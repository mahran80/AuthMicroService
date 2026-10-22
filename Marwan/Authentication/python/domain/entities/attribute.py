from dataclasses import dataclass
from typing import Optional
from modules.architecture.domain.value_objects.code import Code
from modules.architecture.domain.enums.attribute_type import AttributeType
from modules.architecture.domain.value_objects.localization import Localization
from modules.architecture.domain.value_objects.constraints import Constraints
from modules.architecture.domain.value_objects.metadata_view import MetadataView
from modules.architecture.domain.value_objects.clasification import Clasification
from modules.architecture.domain.value_objects.lookup import Lookup

@dataclass
class Attribute:
    code: Code
    icon: str | None
    type: AttributeType
    localization: Localization
    clasification: Clasification
    constraints: Constraints | None
    lookup: Lookup | None
    view: MetadataView
    default: Optional[str] = None




    