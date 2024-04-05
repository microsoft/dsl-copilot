locals {
    suffix = "008"
}

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.97.1"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "resource_group" {
  name     = "rg-${var.prefix}-${local.suffix}"
  location = "East US"
}

module "networking" {
  source                        = "./networking"

  resource_group_name           = azurerm_resource_group.resource_group.name
  resource_group_location       = azurerm_resource_group.resource_group.location
  network_security_group_name   = "nsg-${var.prefix}-${local.suffix}"
  virtual_network_name          = "vnet-${var.prefix}-${local.suffix}"
  virtual_network_address_space = var.virtual_network_address_space
  default_subnet_address_prefix = var.default_subnet_address_space
}

module "ai_search" {
  source                    = "./ai-search"

  resource_group_name       = azurerm_resource_group.resource_group.name
  resource_group_location   = azurerm_resource_group.resource_group.location
  name                      = "ai-${var.prefix}-${local.suffix}"
  sku                       = var.ai_search_sku
  virtual_network_id        = module.networking.virtual_network_id
  virtual_network_name      = module.networking.virtual_network_name
  subnet_address_prefix     = var.ai_search_subnet_address_space

  depends_on = [ 
    module.networking 
  ]
}

module "openai" {
  source                    = "./openai"

  resource_group_name       = azurerm_resource_group.resource_group.name
  resource_group_location   = azurerm_resource_group.resource_group.location
  cognitive_account_name    = "cog-${var.prefix}-${local.suffix}"
  cognitive_account_sku     = "S0"
  openai_account_name       = "openai-${var.prefix}-${local.suffix}"
  openai_account_sku        = var.openai_sku
  openai_model_format       = "OpenAI"
  openai_model_name         = var.openai_model_name
  openai_model_version      = var.openai_model_version
  virtual_network_id        = module.networking.virtual_network_id
  virtual_network_name      = module.networking.virtual_network_name
  subnet_address_prefix     = var.openai_sku

  depends_on = [ 
    module.networking 
  ]
}

module "shared_resources" {
  source = "./shared"

  resource_group_name = azurerm_resource_group.resource_group.name
  resource_group_location = azurerm_resource_group.resource_group.location
  storage_account_name = "st${var.prefix}${local.suffix}"
  storage_account_tier = "Standard"
  storage_account_replication_type = "LRS"
  virtual_network_id = module.networking.virtual_network_id
  virtual_network_name = module.networking.virtual_network_name
  subnet_id = module.networking.default_subnet_id

  depends_on = [ 
    module.networking 
  ]
}

module "chunking_function" {
  source = "./function"

  resource_group_name = azurerm_resource_group.resource_group.name
  resource_group_location = azurerm_resource_group.resource_group.location
  name = "fn-${var.prefix}-${local.suffix}"
  function_storage_account_name = "stfunc${var.prefix}${local.suffix}"
  function_app_service_plan_name = "fn-${var.prefix}-${local.suffix}"
  virtual_network_id = module.networking.virtual_network_id
  virtual_network_name = module.networking.virtual_network_name
  subnet_address_prefix = var.function_subnet_address_space
  private_endpoint_subnet_id = module.networking.default_subnet_id
}

module "app_service" {
  source = "./app-service"

  resource_group_name = azurerm_resource_group.resource_group.name
  resource_group_location = azurerm_resource_group.resource_group.location
  name = "app-${var.prefix}-${local.suffix}"
  app_service_plan_name = "asp-web${var.prefix}-${local.suffix}"
  virtual_network_id = module.networking.virtual_network_id
  virtual_network_name = module.networking.virtual_network_name
  subnet_address_prefix = var.web_app_subnet_address_space
  private_endpoint_subnet_id = module.networking.default_subnet_id
}