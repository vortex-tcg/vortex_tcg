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
 - ### 3 : Error
   - 3.1 : Where to find error
   - 3.2 : Common errors
   - 3.3 : Most particular cases
 - ### 4 : Recommanded Software (Optionnals)


## 1 : Infrastructure :
   The docker structure, is the following :   
   Create a workdir : **Workspace** this workdir while containerized all our app as the same structure
   The goal is to reproduce the working environment.

   ### 1.1 : Game
   This is the first App (WebApp), it has its own **Dockerfile**, it runs on docker port : **8000**, the docker will
   *update* and install *inotify-tools* (this permit to hot reload the container when an update is made in the code
   base for local development).
   
   ### 1.2 : Auth 
   This is the second App (WebApi), this app got a **Dockerfile** too. This one while run on port : **5238**, this on
   will also create a child dir **auth** for match the environment. Like **Game** it use an update to check if file as 
   changes to hot reload the project.
   
   ### 1.3 : Database & Adminer
   This will run the database (MariaDB) in a container expose on port : **3306** and is the first service to run, it as 
   a volume connected named : **mariadb_data**.  
   Many other services depends on the database, so we make sur database run correctly.  

   An Adminer is also provided to acces to the database from local browser, it is expose on docker port : **8080**

   ### 1.4 : SonarQube
   SonarQube is a local tool that allow to see many interesting KPI (security issue, code duplicated, etc...) furthermore
   SonarQube will be integrated to the CI/CD to easily check the test reports. It is expose on docker ports : **9000**
   
   ### 1.5 : Docker Compose
   The docker compose of our app is split in 5 services (details just above) and in volumes to keep data (Database for 
   example). It is also the file that will connect our dockerfile to each others and to our env variables.


## 2 : Launch :
This part will explain how to launch and debug the docker environment by yourselfs in case of errors.

   ### 2.1 : .env
   The .env file is necessary to launch the docker compose.
   It must be in the following dir : *server*, you must provide in this one the following variables : 
   - ```AUTH_PORT``` that correspond to the port where your **auth app** will be expose in your computer
   - ```GAME_PORT``` that correspond to the port where your **game app** will be expose in your computer
   - ```SONARQUBE_PORT``` that correspond to the port where your **local Sonarqube** will be expose in your computer
   - ```DB_PORT``` that correspond to the port where your **database** will be expose in your computer
   - ``ADMINER_PORT`` that correspond to the port where your **database adminer** will be expose in your computer
   - ```DB_USERNAME``` correspond to your **default username** for your local database
   - ```DB_PASSWORD``` correspond to the **password of your default username** for your local database
  
   ### 2.2 : Basics Commands

   - ```docker compose up -d``` use to execute all the services in order (began with database) you need a docker instance
   to run in your computer to use it
   - ```docker compose down``` use to down all the docker instance, and delete it (delete the instance but **not the volumes**)
   - ```docker compose up -d --build``` an alternative to the very first command that is used to rebuild all the images
   it is **required to do it each time a dockerfile is changes ! I also advice to do it after each merge and branch creation
   to avoid potentials errors due to an old version**
   - ```docker system prune``` basic commands that will delete all unused docker images (and clear docker cache). Think
   about using it when a build do not work as expected.

   ### 2.3 : Make Migrations
   The migrations are made by the following dir : ```server > shared > DataAccess``` it is a class library provided by
   **.NET** and allow data types to be shared between differents apps.  
   To create the migrations file you must access a terminal from any docker app container (game or auth).

   In these you will enter the following commands : 
   ```bash
      cd ../../shared/DataAccess
      dotnet tool restore
      dotnet ef migrations add InitDb --project /workspace/shared/DataAccess/DataAccess.csproj --startup-project /workspace/apps/game
   ```

   In order this commands will : 
   - Checkout to the good directory ```server > shared > DataAccess```
   - Install differents dotnet tools (like EF Core necessary to interact with our models)
   - create a new migration (here name : *InitDb*) and start it up from our main projet *Game*

   If anything goes right you must have a success message, you can now go to the next step to apply your migrations.

   ### 2.4 : Update Migrations
   Once you create your migrations or get it from another branch, you must apply it, if you just finish the step 
   [2.3 : Make Migrations], you can just enter the following command (still in you docker terminal)

   ```bash
   dotnet ef database update --project /workspace/shared/DataAccess/DataAccess.csproj --startup-project /workspace/apps/game
   ```
   You can see that this commands is quiet similar to the last one, except that this one do not create migrations, but
   just apply it.

   Once its done, you can reload your database to see the changes.

   ### NB :
   The part 2.3 and 2.4 will be remove when the CI/CD will be finish; the migrations (creation and update) will be
   automatic.  

   If you encounter any trouble with your database migrations and update you can contact @Valentin or @Maxime to help
   you.
   

## 3 : Errors :

   ### 3.1 : Where to find errors
   Thanks to differents *Dockerfile* you can easily find errors depending to which app you are using. You just have to
   open your Docker logs to see and historic of diffents things that happen on you docker container.  
   In more a docker software allow to see quiclky which container run or not.

   ### 3.2 : Commons errors
   This part will cover the most common errors you can found in one of your docker.

   - **Env error** :
      ```The "ENV" variable is not set. Defaulting to a blank string. ```
     This error is about a missing environment variable, you must provide one to make it work correcly (reminder, the 
     .env.example file list every necessary env variable).

   - **Port Already in use** :
     This error is raise when you try to define a port for one of your container that is already in use, you simply need
     to switch to an unused port.
     To easily found which port is in used you can do this commands :  

      #### Windows :
      ```netstat -an | findstr LISTEN```

      #### MacOs :
      ```lsof -nP -iTCP -sTCP:LISTEN```

      #### Linux :
      ```sudo netstat -tulnp```

   - **No docker Daemon is running** :
     This error explain you that you just forgot to launch your docker software (Docker desktop or OrbStack for example)

   ### 3.3 : Most particular cases
   It is also possible for you to encounter a specific error that you never saw before, in this case you can contact @maxime
   he will help you to understand where the error come from.

## 4 : Recommended Software :
   To conclude I also advice you to download : **OrbStack** that is a good docker runner and is really good to
   use with commands prompt for example.  
   
   In more, I didn't recommend you to use a database provider, the fact is that adminer is set up in this case.
   I also advice to use a git GUI (github Desktop or Git Kraken) that is really usefull to understand easily our branching
   strategy.  
   
   Finally, I recommend you to read the docker documentation or to search a bit on the internet if you have questions, this is
   a good practice to develop skills in docker use.