variable "AWS_ACCESS_KEY_ID" {
  description = "AWS Access key"
  type = string
  sensitive = true
}

variable "AWS_SECRET_KEY" {
  description = "AWS Secret key"
  type = string
  sensitive = true
}

variable "DB_ENGINE" {
  description = "Database engine used"
  type = string
}

variable "DB_ENGINE_VERSION" {
  description = "Database engine version used"
  type = string
}

variable "INSTANCE_CLASS" {
  description = "Define the AWS instance use to run the DB"
  type = string
}

variable "DB_NAME" {
  description = "Define DB name"
  type = string
}

variable "DB_USERNAME" {
  description = "Define the default username for the db"
  type = string
  sensitive = true
}

variable "DB_PASSWORD" {
  description = "Define the default password for the db"
  type = string
  sensitive = true
}