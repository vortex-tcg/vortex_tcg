# Terraform & Providing infrastructure

---

## Summary :

- ### 1 : Infrastructure
  - 1.1 : Security
  - 1.2 : Database
  - 1.3 : Cluster
  - 1.4 : Variables
  - 1.5 : Main

- ### 2 : Launch
  - 2.1 : .tfvars
  - 2.2 : commands


This readme while explain in detail how the devops of **Vortex** is working
Terraform while allow to provide resources in AWS.

The goal of this working method is to get easily resources from any cloud provider
and easily add / remove / manage resources without going on aws web site.

In more this **IAC** method will allow to check directly in the code every resources
we need.

## 1 : Infrastructure :
  All about our devops architecture is in this directory, I decided to split our resources
  in many files to keep the readability.
  Here is a little summary about all the files that provide resources.
  
  ### 1.1 : Security : 
  This file is about the account of all the members of the team and the
  policies (who can access what).
  Every new member, or new need about a policy or auth will be added here.
  
  ### 1.2 : Database : 
  This file will contain all the content about providing an instance of database in EC2.
  The majority of variable used in this file are private due to a risk of security.
  You can find the variable list in the **terraform.tfvars**.
  We took a 10GB of allocated storage, it is enough for the project, and stay in the free AWS tier.
  About the engine, we are in **mariadb**, and its engine version is **11.4.5**
  About the password, *Maxime* si the only that can provide the RDS cluster and connect with credentials.
  
  ### 1.3 : Cluster : 
  This file will contain all the code to provide a EC2 Cluster (for the back-end server).
  The first thing in this file is the **data** aws_ami that allow to get the last version
  of an AMI (Amazon Machine Image).
  We do a filter, that will take as value a default string to get the latest version.
  Then we provide this instance, and the **instance_type** is t3.micro again because we have a
  little app and it is include in the aws free tier.

  ### 1.4 : Variables : 
  This file is one of the more important on our app.
  In this file we will define all the variables we will use in ours **.tsvar** file.

  In terraform a .tfvar file is the equivalent of a .env file. We only need to define the 
  variable list before, to set a list of possible existing variables.

  For each variable there is 4 things to keep in mind :
  - the name -> must be in capitalize letters, separate with underscores.
  - description -> the value to describe what this variable is made for.
  - type -> to add more security about the variable and define which type is allow.
  - sensitive -> if set to true, the value will not appear in any logs. (require for password for example)

  To recap, if you need to add a new variable : 
  - You check if the place where you want to put a variable isn't hard coded (and might change)
  - you create your **variable** in the *variables.tf* file with all the needed informations.
  - you add your variable to the *terraform.tfvars.example* like this the others developers would see the new variable you add
  - Next you can go to 2.2 to see the command list.

  ### 1.5 : Main : 
  This file is the first terraform will execute. Actually all our code could be done in this file, but
  I decide to split it in many files to keep visibility. 
  The first thing this file does, is to declare a terraform working env, and inside we will list all the providers
  needed for our project (in our case, we only need **aws**) and we precise the provider version.

  Then we can declare this provider, were you need three parameters : 
  - region (in our case **eu-west-3** - Paris, to reduce loading cost)
  - access_key, that is the key given by the aws administrator (*Maxime*)
  - secret_key, same thing, work as a password

  about the aws access and secret key, you can add them in the *.tfvars* or add it as env variable (in your computer).

## 2 : Launch :
  In this part i will describe everything about launching the devops part.

  ### 2.1 : .tfvars : 
  there is two *.tfvars* files in the devops project : 
  - example
  - normal

  the example is the *.tfvars* that will be share between dev to get the content you have to add in the *.tfvars*
  As say previously the .tfvars is similar to a .env file and contain all the sensitive value of our project.

  you have to create the *terraform.tfvars* if you don't have one, this is from this file that 
  terraform will look for the variables.

  ### 2.2 : Commands : 
  This part will describe all the terraform commands that are necessary to launch and destroy our resources : 
  - terraform init -> will prepare the directory to communicate with the provider
  - terraform plan -> will describe the list of incoming changes 
  - terraform apply -> will apply the changes in the provider
  - terraform destroy -> will kill every process running in the provider

If you encounter any issues or questions contact *Maxime*
  
  