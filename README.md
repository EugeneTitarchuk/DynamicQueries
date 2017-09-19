# DynamicQueries

Allow to search, filter, order IQueryable collections
This library propose following methods:

DynamicSearchBy
DynamicFilterBy
DynamicOrderBy

Core of library is Expression Builders: they are responsive of parsing incoming field 
and building appropriate lambda expressions for Search, Order, Filter

all input parameters are case insensetive
Field Parsing:
    'name'

Functions:
    'count' ('length')
    'any' 

Operations:
    "==" ("Equality")
    "!=" ("Inequality")
    ">" ("GreaterThan")
    "<" ("LessThan")
    ">=" ("GreaterThanOrEqual")
    "<=" ("LessThanOrEqual")



# Parse examples:

Collection object:
    parent: {
        name: "FirstName;
        'child': {
            name: 'FirtsName';
            books: [
                {
                    title: "Book1",
                    pages: ["page1", "page2", "page3"]
                },
                {
                    title: "Book2",
                    pages: ["page1"]
                }
            ]
        }
    }

Field example:
    'name' - name property of input object
    'child.name' - name property of internal object
    'child.books.count' - 'count' functions of items collection of internal object
    'child.books[].title' - title property of each object of items collection of internal object
    'child.books[].pages.count' - count of internal collection of object inside collection


# Order:

public class OrderModel
{
    public OrderItem[] OrderItems { get; set; }
}

public class OrderItem
{
    public string Field { get; set; }
    public string Direction { get; set; }
}

in json:
{
    orderItems: [
        field: ""
        direction: ""
    ]
}

direction variants: "acs", "ascending", "desc", "descending"

# Filter:

public class FilterModel
{
    public FilterItem[] FilterItems { get; set; }
}

public class FilterItem
{
    public string Field { get; set; }
    public string Value { get; set; }
    public string Operation { get; set; }
}

in json:

{
    filterItems: [
        field: "",
        value: "",
        operation: ""
    ]
}

operation - not reqired - '==' by default. To internal collections will be applied function "All"


# Search

public class SearchModel
{
    public string[] Fields { get; set; }
    public string Value { get; set; }
}

in json:

{
    filterItems: [
        fields: [],
        value: ""        
    ]
}

To internal collections will be applied function "Any"
