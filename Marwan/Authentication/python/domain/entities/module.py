from dataclasses import dataclass, field

from modules.architecture.domain.value_objects.code import Code
from modules.architecture.domain.value_objects.localization import Localization

from .aggregate import Aggregate


@dataclass
class Module:
    code: Code
    localization: Localization

    icon: str | None = None

    aggregates: list[Aggregate] = field(
        default_factory=list
    )