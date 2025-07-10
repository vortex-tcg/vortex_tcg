# Using Docker :

---

## Summary : 

 - ### 1 : Infrastructure
   - 1.1 : Game
   - 1.2 : Auth
   - 1.3 : Database
   - 1.4 : Adminer
   - 1.5 : SonarQube
   - 1.6 : Docker Compose
 - ### 2 : Launch
   - 2.1 : .env
   - 2.2 : Basics Commands
   - 2.3 : Make migrations
   - 2.4 : Update migrations
 - ### 3 : 
 - ### 4 : Recommand Software (Optionnals)


## 1 : Infrastructure :
   The docker structure, is the following : 
   Create a workdir : **Workspace** this workdir while containerized all our app as the same structure
   The goal is to reproduce the working environnement.

   ### 1.1 : Game
   This is the first App (WebApp), it as its own **Dockerfile**, it run on docker port : **8000**, the docker will
   *update* and install *inotify-tools* (this permit to hot reload the container when and update is made in the code
   base for local development).
   
   ### 1.2 : Auth 
   This is the second App (WebApi), this app got a **Dockerfile** too. This one while run on port : **5238**, this on
   will also create a child dir **auth** for match the environment. Like **Game** it use an update to check if file as 
   changes to hot reload the project.
   
   ### 1.3 : Database & Adminer
   This will run the database (MariaDB) in a container expose on port : **3306** and is the first service to run, it as 
   a volume connected named : **mariadb_data**.
   Many other services depends on the database, so we make sur database run correcly.
   An Adminer is also provided to acces to the database from local browser, it is expose on docker port : **8080**

   ### 1.4 : SonarQube
   SonarQube is a local tool that allow to see many interesting KPI (security issue, code duplicated, etc...) furthermore
   SonarQube will be integrated to the CI/CD to easily check the test reports. It is expose on docker ports : **9000**
   
   ### 1.5 : Docker Compose
   The docker compose of our app is split in 5 services (details just above) and in volumes to keep data (Database for 
   example).
   The basics commands to know are the following : 
   
   - ```docker compose up -d``` use to execute all the services in order (began with database) you need a docker instance
   to run in your computer to use it
   - ```docker compose down``` use to down all the docker instance, and delete it (delete the instance but **not the volumes**)
   - ```docker compose up -d --build``` an alternative to the very first command that is used to rebuild all the images
   it is **required to do it each time a dockerfile is changes ! I also advice to do it after each merge and branch creation
   to avoid potentials errors due to an old version**
   - ```docker system prune``` basic commands that will delete all unused docker images (and clear docker cache). Think
   about using it when a build do not work as expected.

   To conclude this part I also advice you to download : **OrbStack** that is a good docker runner and is really good to
   use with commands prompt for example.