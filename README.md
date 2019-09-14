# SquadRconTool
This application was mainly created for the EquinoxGamers.com community.

# Server Structure
The server is planned to be built as an "interface" between an actual server, and the connected clients. Free software.
Only the server knows the actual RCON password to the Squad server(s) therefore the application would provide the following features:
* Authentication with a user / password combination given by a master administrator. (Server owner)
* Permission system to control authenticated user's access.
* Encryption.
* Performable tasks such as map votes, and more.
* Logs of authentication, commands, chat, and more.

# Client Structure
The client is a normal client that is not able to communicate through Source RCON protocols, but is able to enstablish connection to any SquadRconTool servers. This way we ensure that the client is going to use an "interface" which will decide what access will you have to the server. It would obviously make administration a lot more easier.
