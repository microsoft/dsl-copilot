variable "resource_group_name" {
  type          = string
  description   = "Name of the resource group in which to create the openai resources"  
}

variable "resource_group_location" {
  type          = string
  description   = "Location of the resource group in which to create the openai resources"    
}

variable "cognitive_account_name" {
  type          = string
  description   = "Name of the cognitive services account resource"  
}

variable "cognitive_account_sku" {
  type          = string
  description   = "SKU of the cognitive services account resource"  
}

variable "openai_account_name" {
  type          = string
  description   = "Name of the openai account resource"
}

variable "openai_account_sku" {
  type          = string
  description   = "SKU of the openai account resource"
}

variable "openai_model_format" {
  type          = string
  description   = "Format of the openai resource"  
}

variable "openai_model_name" {
  type          = string
  description   = "Model name of the openai resource"  
}

variable "openai_model_version" {
  type          = string
  description   = "Version of the openai resource"    
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