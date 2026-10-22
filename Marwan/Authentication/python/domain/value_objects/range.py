from dataclasses import dataclass
from typing import Optional

@dataclass(frozen=True)
class Range:
    min: Optional[float] = None
    max: Optional[float] = None

    def is_valid(self, value: float) -> bool:
        if self.min is not None and value < self.min:
            return False
        if self.max is not None and value > self.max:
            return False
        return True