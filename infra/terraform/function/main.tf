resource "azurerm_storage_account" "function_storage_account" {
  name                     = var.function_storage_account_name
  resource_group_name      = var.resource_group_name
  location                 = var.resource_group_location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_service_plan" "function_service_plan" {
  name                = var.function_app_service_plan_name
  location            = var.resource_group_location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = "EP1"
}

resource "azurerm_linux_function_app" "function_app" {
  name                          = var.name
  location                      = var.resource_group_location
  resource_group_name           = var.resource_group_name
  service_plan_id               = azurerm_service_plan.function_service_plan.id
  storage_account_name          = azurerm_storage_account.function_storage_account.name
  storage_account_access_key    = azurerm_storage_account.function_storage_account.primary_access_key
  public_network_access_enabled = false
  virtual_network_subnet_id     = azurerm_subnet.function_subnet.id

  site_config {
    application_stack {
        python_version  = "3.11"
    }
  }
}

resource "azurerm_subnet" "function_subnet" {
  name                 = "function-subnet"
  resource_group_name  = var.resource_group_name
  virtual_network_name = var.virtual_network_name
  address_prefixes     = [var.subnet_address_prefix]

  delegation {
    name = "delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
    }
  }
}

resource "azurerm_private_endpoint" "function_subnet_private_endpoint" {
  name                = "pe-function_subnet"
  location            = var.resource_group_location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "psc-function_subnet"
    private_connection_resource_id = azurerm_linux_function_app.function_app.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}