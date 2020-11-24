# BinaryTableDB

## Description
**BinaryTableDB**, or from now on *"BTable"*, is a **local database file architecture**, in which data is represented by **columns and rows**.

But unlike CSV files, BTables are completely **binary**, giving a huge advantage in read/write speed. 

And while they are also **not compressed**, the size of the rows are always fixed, meaning a **fast random access**, as the position of a desired row can be easily calculated.

## File Structure Specification
There are two main block types in a BTable, the **head**, and the **body**, but we are also going to explain how strings are encoded.

### <ins>Header</ins>
All BTable files have headers, displaying information in the following structure:

- **Number of columns:** <ins>8bit integer</ins>. Total amount of columns. Maximum 256 columns.
- **Column name:** <ins>String</ins>. Name and identifier. Used to reference a cell in a row.<br>
*This can also be done by providing a column index, in fact, using column indexes are much faster.*
- **Column width:** <ins>8bit integer</ins>. Maximum width for all the cells in the column. Maximum 256 bytes.

### <ins>String</ins>
Binary strings in BTables are represented in file as it follows:
- **Character count:** <ins>8bit integer</ins>. Maximum 255 bytes/characters.
- **Character array:** <ins>Array</ins>. The string itself.

### <ins>Body</ins>
The body contains the BTable rows, and are stored as the header describes for each file.

*For example: A body for a 16 and 256 byte header columns widths, would be a corresponding binary data space of 16 bytes and 256 bytes.*