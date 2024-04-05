variable "resource_group_name" {
  type          = string
  description   = "Name of the resource group in which to create the shared resources"
  
}

variable "resource_group_location" {
  type          = string
  description   = "Location of the resource group in which to create the shared resources"  
}

variable "storage_account_name" {
  type          = string
  description   = "Name of the storage account resource"
}

variable "storage_account_tier" {
  type          = string
  description   = "Tier of the storage account resource"  
}

variable "storage_account_replication_type" {
  type          = string
  description   = "Replication type of the storage account resource"  
}

variable "virtual_network_id" {
  type          = string
  description   = "ID of the virtual network to which the private endpoint will be connected"
}

variable "virtual_network_name" {
  type          = string
  description   = "Name of the virtual network to which the private endpoint will be connected"
}

variable "subnet_id" {
  type          = string
  description   = "ID of the subnet to be created for the ai search resource"
}