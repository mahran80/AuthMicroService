from enum import Enum

class AttributeType(str, Enum):
    STRING = "string"
    NUMBER = "number"
    FLOAT = "float"
    DATE = "date"
    DATETIME = "datetime"
    BOOLEAN = "boolean"
    UUID = "uuid"
    OBJECT = "object"
    ARRAY = "array"
    ENUM = "enum"
    VALUE_OBJECT = "value_object"