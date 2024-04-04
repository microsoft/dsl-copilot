resource "azurerm_search_service" "ai_search_service" {
  name                = var.name
  resource_group_name = var.resource_group_name
  location            = var.resource_group_location
  sku                 = var.sku
  public_network_access_enabled = false
}

resource "azurerm_subnet" "ai_search_subnet" {
  name                 = "aisearch-subnet"
  resource_group_name  = var.resource_group_name
  virtual_network_name = var.virtual_network_name
  address_prefixes     = [var.subnet_address_prefix]
}

resource "azurerm_private_dns_zone" "ai_search_dns_zone" {
  name                = "privatelink.search.windows.net"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_zone_virtual_network_link" "ai_search_vnet_link" {
  name                  = "vnet-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.ai_search_dns_zone.name
  virtual_network_id    = var.virtual_network_id
}

resource "azurerm_private_endpoint" "ai_search_private_endpoint" {
  name                = "pe-aisearch"
  location            = var.resource_group_location
  resource_group_name = var.resource_group_name
  subnet_id           = azurerm_subnet.ai_search_subnet.id

  private_service_connection {
    name                           = "psc-aisearch"
    private_connection_resource_id = azurerm_search_service.ai_search_service.id
    is_manual_connection           = false
    subresource_names              = ["searchService"]
  }
}