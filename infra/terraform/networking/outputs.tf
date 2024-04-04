output "virtual_network_name" {
  value = azurerm_virtual_network.virtual_network.name  
}

output "virtual_network_id" {
  value = azurerm_virtual_network.virtual_network.id
}

output "default_subnet_id" {
  value = azurerm_subnet.default_subnet.id
}