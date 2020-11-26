# BinaryTableDB

## Description
**BinaryTableDB**, or from now on *"BTable"*, is a **local database file architecture**, in which data is represented by **columns and rows**.

But unlike CSV files, BTables are completely **binary**, giving a huge advantage in read/write speed. 

And while they are also **not compressed**, the size of the rows are always fixed, meaning a **fast random access**, as the position of a desired row can be easily calculated.

## File Structure Specification
There are two main block types in a BTable, the **head**, and the **body**.

### <ins>Header</ins>
All BTable files have headers, displaying information in the following structure:

| Data                  | Type          | Size          | Description |
| -------------         | ------------- | ------------  | ----------- |
| Number of columns     | Integer       | 8bit          | Total amount of columns |
| Column name           | String        | Variable      | Name and also an identifier |
| Column width          | Integer       | 8bit          | Maximum width for all the cells in the column |

*It is recommended to reference a column by providing a column index, it will result in a much faster seek.*

### <ins>Body</ins>
The body contains the BTable rows, and are stored as the header describes for each file.

*For example: A body for a 16 and 256 byte header columns widths, would be a corresponding binary data space of 16 bytes and 256 bytes.*