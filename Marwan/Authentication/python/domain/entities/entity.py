from dataclasses import dataclass
from typing import Optional
from modules.architecture.domain.value_objects.code import Code
from modules.architecture.domain.value_objects.localization import Localization
from modules.architecture.domain.value_objects.clasification import Clasification


@dataclass
class Entity:
    code: Code
    icon: str | None
    localization: Localization
   






  