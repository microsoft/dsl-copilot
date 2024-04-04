variable "prefix" {
  type          = string
  description   = "Value used to prefix all resources created by this module"
  default = "pmtest"
}

variable "virtual_network_address_space" {
  type          = list(string)
  description   = "Address space for the virtual network"
  default = ["10.0.0.0/16"]  
}

variable "default_subnet_address_space" {
  type          = string
  description   = "Address space for the default subnet"
  default = "10.0.1.0/24"
}

variable "ai_search_sku" {
  type          = string
  description   = "SKU for the Azure Cognitive Search service"
  default = "standard"
}

variable "ai_search_subnet_address_space" {
  type          = string
  description   = "Address space for the subnet hosting the Azure Cognitive Search service"
  default = "10.0.2.0/24"
}

variable "openai_sku" {
  type          = string
  description   = "SKU for the Azure Cognitive Search service"
  default = "Standard"
}

variable "openai_model_name" {
  type          = string
  description   = "Name of the OpenAI model to deploy"
  default = "gpt-35-turbo"
}

variable "openai_model_version" {
  type          = string
  description   = "Version of the OpenAI model to deploy"
  default = "0301"
}

variable "openai_subnet_address_space" {
  type          = string
  description   = "Address space for the subnet hosting the OpenAI service"
  default = "10.0.3.0/24" 
}

variable "function_subnet_address_space" {
  type          = string
  description   = "Address space for the subnet hosting the function app"
  default = "10.0.4.0/24"
}

variable "web_app_subnet_address_space" {
  type          = string
  description   = "Address space for the subnet hosting the web app"
  default = "10.0.5.0/24"
}