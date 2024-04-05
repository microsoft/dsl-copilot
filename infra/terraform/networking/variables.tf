variable "resource_group_name" {
  type          = string
  description   = "Name of the resource group in which to create the networking resources"
  
}

variable "resource_group_location" {
  type          = string
  description   = "Location of the resource group in which to create the networking resources"  
}

variable "network_security_group_name" {
  type          = string
  description   = "Name of the network security group resource"
}

variable "virtual_network_name" {
  type          = string
  description   = "Name of the virtual network resource"
}

variable "virtual_network_address_space" {
  type          = list(string)
  description   = "Address space of the virtual network resource"
}

variable "default_subnet_address_prefix" {
  type          = string
  description   = "Address prefix of the subnet to be created for the ai search resource"
}