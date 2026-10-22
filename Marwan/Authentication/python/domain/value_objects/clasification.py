from dataclasses import dataclass
from modules.architecture.domain.value_objects.lookup import Lookup

@dataclass(frozen=True)
class Clasification:
    parent_attribute_code: str
    parent_entity_code: str
    parent_feature_code: str
    parent_module_code: str
    
    is_foreign_key: bool = False
    enum: list[str] | None = None
    lookup: Lookup | None = None

    is_indexed: bool = False
    is_partitioned: bool = False
