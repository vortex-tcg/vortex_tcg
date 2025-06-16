terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
      version = "5.99.1"
    }
  }
}

provider "aws" {
  # region choise -> Paris, nearest
  region = "eu-west-3"
  access_key = var.AWS_ACCES_KEY_ID
  secret_key = var.AWS_SECRET_KEY
}