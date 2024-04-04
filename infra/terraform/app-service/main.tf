resource "azurerm_service_plan" "app_service_plan" {
  name                = var.app_service_plan_name
  location            = var.resource_group_location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = "P1v3"
}

resource "azurerm_linux_web_app" "web_app" {
  name                          = var.name
  location                      = var.resource_group_location
  resource_group_name           = var.resource_group_name
  service_plan_id               = azurerm_service_plan.app_service_plan.id
  public_network_access_enabled = false
  virtual_network_subnet_id     = azurerm_subnet.web_app_subnet.id

  site_config {
    application_stack {
        dotnet_version  = "8.0"
    }
  }
}

resource "azurerm_subnet" "web_app_subnet" {
  name                 = "webapp-subnet"
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


resource "azurerm_private_endpoint" "app_subnet_private_endpoint" {
  name                = "pe-app_subnet"
  location            = var.resource_group_location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "psc-app_subnet"
    private_connection_resource_id = azurerm_linux_web_app.web_app.id
    is_manual_connection           = false
    subresource_names              = ["sites"]
  }
}