# BinaryTableDB

## Description
**BinaryTableDB**, or from now on *"BTable"*, is a **local database file architecture**, in which you are free to code your **own interpreter**, meaning that you are responsible about how exactly the data is encoded in a stream. This gives speed advantages as there will be very little overhead compared to other solutions.

We can compare the data structure to be like a CSV file, but in completely **binary** version.

In order to specify encoding/decoding procedures you must implement *ICustomSerializable* to the class representing a row.

## Data Specification
There are two different blocks of data in every BTable file version 1, **header** and **body**.

### Header
| Name | Size | Type | Value | Description |
| ---- | ---- | ---- | ----- | ----------- |
| Signature | 3-byte | String | BT | BTable signature header
| Version   | 1-byte | Integer | 1 | Data-structure version
| Row width | 4-byte | Integer | Variable | The number of bytes every row  has

### Body
The body holds all the rows the BTable has. There can be infinite amount of rows.