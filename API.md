# Hideez Enterprise Server web API Quick Start
The HES web API is intended to integrate the Hideez Enterprise Solution with the third party services. Using the API you can manage employees, devices, and credentials. Almost everything you can do with the HES web interface you can do via the HES REST API calls.
To see the list of available methods, their parameters and return values you need to install your own HES instance and open the link: `https://<your_server_name>/swagger/` By this link, you will be able to test each method as well. To open the link you need to logon the HES.   

To use web API calls from an external application or service you need to authorize first. 
Use `POST` `/api/Identity/Login` and send your login (email) and password in the json format:

    ```json
    {
        "email": "<user_email>",
        "password": "<user_password>"
    }
    ```
Server response headers will contain cookies named '.AspNetCore.Identity.Application'. Add these cookies to the headers of each following request.

