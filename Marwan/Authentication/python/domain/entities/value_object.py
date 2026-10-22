from dataclasses import dataclass, field

from modules.architecture.domain.value_objects.code import Code
from modules.architecture.domain.value_objects.localization import Localization

from .attribute import Attribute


@dataclass
class ValueObject:
    code: Code
    localization: Localization

    attributes: list[Attribute] = field(
        default_factory=list
    )