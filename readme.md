# BinaryTableDB

## Description
**BinaryTableDB**, or from now on *"BTable"*, is a **local database file architecture**, in which you are free to code your **own interpreter**, meaning that you are responsible about how exactly the data is encoded in a stream. This gives speed advantages as there will be very little overhead compared to other solutions.

We can compare the data structure to be like a CSV file, but in completely **binary** version.

Implementing *ICustomSerializable* will enable you with 

## Data Specification

### Header
| Name | Size | Type | Value | Description |
| ---- | ---- | ---- | ----- | ----------- |
| Signature | 2-byte | String | BT | BTable signature header
| Version   | 1-byte | Integer | 1 | Data-structure version
| Row width | 4-byte | Integer | Variable | The number of bytes every row  has

### Body
- The entries stored after the header needs to be encoded/decoded implementing *ICustomSerializable*.
- Each entry cannot exceed the row width.