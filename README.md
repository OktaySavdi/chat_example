We will create a simple application with dotnet-core

we will use the images created here kubernetes_chat_example [Chat-example](https://github.com/OktaySavdi/kubernetes_chat_example)


#  Build

    docker build -t chatproject -f Dockerfile .

# Run
    docker run -d -p 5000:80 --name myapp chatproject

# Call
    curl http://localhost:5000/chat
