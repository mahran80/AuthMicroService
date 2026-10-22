from dataclasses import dataclass

@dataclass
class Localization:
    name: dict[str, str]
    plural_name: dict[str, str]
    description: dict[str, str]