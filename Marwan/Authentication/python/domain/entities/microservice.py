from dataclasses import dataclass, field

from modules.architecture.domain.value_objects.code import Code
from modules.architecture.domain.value_objects.localization import Localization

from .module import Module


@dataclass
class Microservice:
    code: Code
    localization: Localization

    icon: str | None = None

    modules: list[Module] = field(
        default_factory=list
    )