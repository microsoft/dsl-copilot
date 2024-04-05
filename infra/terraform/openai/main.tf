resource "azurerm_cognitive_account" "cognitive_account" {
  name                              = var.cognitive_account_name
  location                          = var.resource_group_location
  resource_group_name               = var.resource_group_name
  kind                              = "OpenAI"
  sku_name                          = var.cognitive_account_sku
  custom_subdomain_name             = var.openai_account_name
  public_network_access_enabled     = false
}

resource "azurerm_cognitive_deployment" "cognitive_deployment" {
  name                 = var.openai_account_name
  cognitive_account_id = azurerm_cognitive_account.cognitive_account.id

  model {
    format  = var.openai_model_format
    name    = var.openai_model_name
    version = var.openai_model_version
  }

  scale {
    type = var.openai_account_sku
  }
}

resource "azurerm_subnet" "openai_subnet" {
  name                 = "openai-subnet"
  resource_group_name  = var.resource_group_name
  virtual_network_name = var.virtual_network_name
  address_prefixes     = [var.subnet_address_prefix]
}

resource "azurerm_private_dns_zone" "openai_dns_zone" {
  name                = "privatelink.openai.azure.com"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_zone_virtual_network_link" "openai_vnet_link" {
  name                  = "vnet-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.openai_dns_zone.name
  virtual_network_id    = var.virtual_network_id
}

resource "azurerm_private_endpoint" "openai_private_endpoint" {
  name                = "pe-openai"
  location            = var.resource_group_location
  resource_group_name = var.resource_group_name
  subnet_id           = azurerm_subnet.openai_subnet.id

  private_service_connection {
    name                           = "psc-aisearch"
    private_connection_resource_id = azurerm_cognitive_account.cognitive_account.id
    is_manual_connection           = false
    subresource_names              = ["account"]
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [azurerm_private_dns_zone.openai_dns_zone.id]
  }
}