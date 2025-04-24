This is a Microservice made using ASP.NET 

It contains two services-
1. OrderService
2. TrackingService

OrderService-
In orderservice, we can place and order with the required details and these orders are stored in OrderDB database(SQLite). 

Functions-
1. Add new order
2. Get tracking info of the order using order_id(this will contact the tracking service).

Whenever a new order is added, The same order's order_id and type(push or track) is pushed to a message queue(Redis is used for message queue).This object is then consumed by the Trackingservice worker and is processed.


TrackingService-
In trackingservice, the tracking details for the orders are store in trackingDB based on the orderid as key.

Functions-
1. Update the location or data(current place where the order is)
2. Listen to the message queues continuously and if there is push message(new order) then add it to the trackingDB and if there is a track message then get the data using orderid from the trackingDB and push it to another queue from where the order service will be listening for the response.