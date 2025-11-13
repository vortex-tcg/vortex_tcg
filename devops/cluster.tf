// This data will allow to get the latest ubuntu image 
data "aws_ami" "ubuntu" {
  most_recent = true
  
  # We filter about "name" and its value is the generical name given
  # By the official provider
  filter {
    name   = "name"
    values = ["ubuntu/images/hvm-ssd/ubuntu-jammy-22.04-amd64-server-*"]
  }

  owners = ["099720109477"]
}

// Create a resource EC2 of type linux ubuntu
resource "aws_instance" "server" {
  ami = data.aws_ami.ubuntu.id
  instance_type = "t3.micro"
  
  tags = {
    Name = "HelloWorld"
  }
}