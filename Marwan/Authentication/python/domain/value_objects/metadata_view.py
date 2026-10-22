
from dataclasses import dataclass

@dataclass
class MetadataView:
    is_public: bool
    order: int
    depth: int
    scope: list[str]