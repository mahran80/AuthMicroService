from dataclasses import dataclass, field

from modules.architecture.domain.value_objects.code import Code
from modules.architecture.domain.value_objects.localization import Localization

from .entity import Entity
from .value_object import ValueObject


@dataclass
class Aggregate:
    code: Code
    localization: Localization

    icon: str | None = None

    entities: list[Entity] = field(
        default_factory=list
    )

    value_objects: list[ValueObject] = field(
        default_factory=list
    )