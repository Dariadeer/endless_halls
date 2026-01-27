```mermaid
sequenceDiagram
	Note over Client,Server: Connection
	Client ->> +Server: Connect to the socket
	Server -->> Client: User data
	Note over Client,Server: Initialization
	Client ->> Server: World join request
	Server -->> Client: World snapshot data
	Server -->> Client: Relevant command data
	Note over Client,Server: Communication
	Client ->> Server: Own commands
	Server -->> Client: AI & other client commands
```
