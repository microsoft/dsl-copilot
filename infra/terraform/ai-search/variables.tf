variable "resource_group_name" {
  type          = string
  description   = "Name of the resource group in which to create the ai search resource"
  
}

variable "resource_group_location" {
  type          = string
  description   = "Location of the resource group in which to create the ai search resource"  
}

variable "name" {
  type          = string
  description   = "Name of the ai search resource"
}

variable "sku" {
  type          = string
  description   = "SKU of the ai search resource"
  default       = "standard"
}

variable "virtual_network_id" {
  type          = string
  description   = "ID of the virtual network to which the private endpoint will be connected"
}

variable "virtual_network_name" {
  type          = string
  description   = "Name of the virtual network to which the private endpoint will be connected"
}

variable "subnet_address_prefix" {
  type          = string
  description   = "Address prefix of the subnet to be created for the ai search resource"
}
