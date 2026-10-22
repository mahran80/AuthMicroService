from enum import Enum

class Occurrence(str, Enum):
    None = "none"
    OPTIONAL = "optional"
    CONDITIONAL = "conditional"
    MANDATORY = "mandatory"
