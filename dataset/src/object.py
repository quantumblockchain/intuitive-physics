from enum import Enum

class Object:

    class ColorType(Enum):
        BLUE = "blue",
        BROWN = "brown",
        CYAN = "cyan",
        GRAY = "gray",
        GREEN = "green",
        PURPLE = "purple",
        RED = "red",
        YELLOW = "yellow",
        BLACK = "black"

    class GeometryType(Enum):
        SMALL_CUBE = "s_cube",
        BIG_CUBE = "b_cube",
        STANDARD_RECTANGLE = "std_rect",
        SMALL_TRIANGLE = "s_tri",
        BIG_TRIANGLE = "b_tri",
        SMALL_HEXAGON = "s_hex",
        BIG_HEXAGON = "b_hex",
        WALL_PIN = "pin",
        ROPE_UNIT = "rp_unit",
        SMALL_CIRCLE = "s_circle",
        BIG_CIRCLE = "b_circle",
        BIG_RAMP = "b_ramp",
        CUSTOM_RECTANGLE = "cus_rectange",
        CAR_BODY = "car_bd",
        CAR_WHEEL = "car_wh",
        ROD_RECTANGLE = "rod",
        BASKET = "basket",
        LEFT_BOUNDARY = "l_bound",
        RIGHT_BOUNDARY = "r_bound",
        BOTTOM_BOUNDARY = "b_bound"


    KEY_ACTIVE = "active"
    KEY_POSX = "posX"
    KEY_POSY = "posY"
    KEY_BODY_TYPE = "bodyType" #bodyType=0 --> static, bodyType=1 --> not used, bodyType=2 --> dynamic
    KEY_UNIQUE_ID = "uniqueID"
    KEY_COLOR_TYPE = "colorType"
    KEY_OBJECT_TYPE = "objectType"


    def __init__(self, initialState, endState):
        self.initialState = initialState
        self.finalState = endState
        self.isDynamic = self.initialState[Object.KEY_BODY_TYPE] == 0
        self.uniqueId = self.initialState[Object.KEY_UNIQUE_ID]
        self.color = Object.ColorType(self.initialState[Object.KEY_COLOR_TYPE])
        self.geometry = Object.GeometryType(self.initialState[Object.KEY_OBJECT_TYPE])

    def isActiveAtInit(self):
        return self.initialState[Object.KEY_ACTIVE]

    def isActiveAtEnd(self):
        return self.finalState[Object.KEY_ACTIVE]

    def posXAtInit(self):
        return self.initialState[Object.KEY_POSX]

    def posXAtEnd(self):
        return self.finalState[Object.KEY_POSX]

    def posYAtInit(self):
        return self.initialState[Object.KEY_POSY]

    def posYAtEnd(self):
        return self.finalState[Object.KEY_POSY]
