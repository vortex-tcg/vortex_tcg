resource "aws_db_instance" "vortex_db" {
  allocated_storage = 10
  # Define the engine and its version
  engine = var.DB_ENGINE
  engine_version = var.DB_ENGINE_VERSION
  # Define the AWS instance use to run the DB
  instance_class = var.INSTANCE_CLASS
  # Define the DB name
  db_name = var.DB_NAME
  # Credentials to connect DB instance 
  username = var.DB_USERNAME
  password = var.DB_PASSWORD
  
  // This parameter will skip a save, so if terraform is down, db while be deleted
  skip_final_snapshot = true
}