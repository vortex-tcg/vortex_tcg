# Terraform & Providing infrastructure

---

## Summary :

- ### 1 : Infrastructure
  - 1.1 : Security
  - 1.2 : Database
  - 1.3 : EC2
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
  
  ### 1.3 : EC2 : 