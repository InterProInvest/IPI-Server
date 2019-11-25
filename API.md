# API documentation
Server authorization based on cookies files.
## Quick Start
- Use `POST` `/api/Identity/Login` to authorization by sending email and password in json format. Server will return cookies.

    ```json
    {
        "email": "<user_email>",
        "password": "<user_password>"
    }
    ```
 - For request, set cookies in the header key `Cookie` and value `.AspNetCore.Identity.Application=...`
 - Use swagger documentation `https://<your_server_name>/swagger/`
