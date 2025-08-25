resource "aws_db_instance" "vortex_db" {
  allocated_storage = 10
  # Define the engine and its version
  engine = ""
  engine_version = "11.8.2"
  # Define the AWS instance use to run the DB
  instance_class = "db.t3.micro"
  # Define the DB name
  db_name = ""
  # Credentials to connect DB instance 
  username = ""
  password = ""
}