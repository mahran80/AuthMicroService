from dataclasses import dataclass
from modules.architecture.domain.enums.occurence import Occurrence

@dataclass
class Constraints:

    occurrence: Occurrence
    
    is_primary_key: bool
    is_nullable: bool
    is_unique: bool
 
    value: range | None
    length: range | None
    repeat: range | None

    regex: str | None
    rules: list[str] | None