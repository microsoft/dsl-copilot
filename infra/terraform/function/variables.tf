variable "resource_group_name" {
  type = string
  description = "The name of the resource group"
}

variable "resource_group_location" {
  type = string
  description = "The location of the resource group"
}

variable "name" {
  type = string
  description = "The name of the function app"
}

variable "function_storage_account_name" {
  type = string
  description = "The name of the storage account for the function app"  
}

variable "function_app_service_plan_name" {
  type = string
  description = "The name of the app service plan for the function app"
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

variable "private_endpoint_subnet_id" {
  type          = string
  description   = "ID of the subnet to which the private endpoint will be connected"  
}