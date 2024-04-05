resource "azurerm_storage_account" "storage_account" {
  name                            = var.storage_account_name
  location                        = var.resource_group_location
  resource_group_name             = var.resource_group_name
  account_tier                    = var.storage_account_tier
  account_replication_type        = var.storage_account_replication_type
  public_network_access_enabled   = false
}

resource "azurerm_private_dns_zone" "storage_dns_zone" {
  name                = "privatelink.blob.core.windows.net"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_zone_virtual_network_link" "storage_vnet_link" {
  name                  = "vnet-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.storage_dns_zone.name
  virtual_network_id    = var.virtual_network_id
}

resource "azurerm_private_endpoint" "storage_private_endpoint" {
  name                = "pe-storage"
  location            = var.resource_group_location
  resource_group_name = var.resource_group_name
  subnet_id           = var.subnet_id

  private_service_connection {
    name                           = "psc-storage"
    private_connection_resource_id = azurerm_storage_account.storage_account.id
    is_manual_connection           = false
    subresource_names              = ["blob"]
  }
}