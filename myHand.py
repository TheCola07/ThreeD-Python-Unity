import mediapipe as mp
import cv2
from cvzone.HandTrackingModule import HandDetector
import math

class handDetector:
    def __init__(self, 
                 mode=False, 
                 maxHands=1,
                 modelComplexity=1, 
                 detectionCon=0.8, 
                 minTrackCon=0.5):
        self.mode = mode
        self.maxHands = maxHands
        self.complexity = modelComplexity
        self.detectionCon = detectionCon
        self.minTrackCon = minTrackCon
        
        
        self.mpHands = mp.solutions.hands   # 获得手部检测的解决方案
        self.hands = self.mpHands.Hands(self.mode, self.maxHands, self.complexity, self.detectionCon, self.minTrackCon)
        self.mpDraw = mp.solutions.drawing_utils # 获得绘制工具方案，用于关键点的绘制
        
        self.lmList = []
        
    def findHand(self, img, is_draw=False):
        
        # 将获得的图像由BGR转到RGB空间
        imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        # 对图像进行手部关键点检测处理
        self.result = self.hands.process(imgRGB)
        # print(result.multi_hand_landmarks)    # 检测多只手的关键点
        
        if self.result.multi_hand_landmarks:
            for handLms in self.result.multi_hand_landmarks: # 遍历检测到的手部关键点
                if is_draw:      
                    self.mpDraw.draw_landmarks(img, handLms, self.mpHands.HAND_CONNECTIONS) # 使用绘制方案(输出图像，手部信息，绘制模式与状态)
        
        return img
    
    def findHands(self, img, is_draw=False, flipType=True):
        imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        self.results = self.hands.process(imgRGB)
        allHands = []           # 整只手的关键点坐标信息
        h, w, c = img.shape
        if self.results.multi_hand_landmarks:
            for handType, handLms in zip(self.results.multi_handedness, self.results.multi_hand_landmarks):
                # 将手的类型和手的关键点绑定成一个元组
                myHand = {}     # 将手的信息处理并作为关键字存储到字典中
                ## lmList 坐标
                mylmList = []
                xList = []
                yList = []
                for id, lm in enumerate(handLms.landmark):
                    px, py, pz = int(lm.x * w), int(lm.y * h), int(lm.z * w)
                    mylmList.append([px, py, pz])
                    xList.append(px)
                    yList.append(py)
                
                ## bbox 边界框
                xmin, xmax = min(xList), max(xList)
                ymin, ymax = min(yList), max(yList)
                boxW, boxH = xmax - xmin, ymax - ymin
                bbox = xmin, ymin, boxW, boxH           # 存储左上角坐标，和边界框的宽度和高度
                cx, cy = bbox[0] + (bbox[2] // 2), \
                         bbox[1] + (bbox[3] // 2)

                myHand["lmList"] = mylmList
                myHand["bbox"] = bbox
                myHand["center"] = (cx, cy)

                if flipType:                            # 图像是否翻转
                    if handType.classification[0].label == "Right":
                        myHand["type"] = "Left"
                    else:
                        myHand["type"] = "Right"
                else:
                    myHand["type"] = handType.classification[0].label
                allHands.append(myHand)
                
                ## draw 绘制
                if is_draw:
                    self.mpDraw.draw_landmarks(img, handLms,
                                               self.mpHands.HAND_CONNECTIONS)
                    cv2.rectangle(img, (bbox[0] - 20, bbox[1] - 20),
                                  (bbox[0] + bbox[2] + 20, bbox[1] + bbox[3] + 20),
                                  (255, 0, 255), 2)
                    cv2.putText(img, myHand["type"], (bbox[0] - 30, bbox[1] - 30), cv2.FONT_HERSHEY_PLAIN,
                                2, (255, 0, 255), 2)

        return allHands, img
        
    def findPosition(self, img, handid=0, is_draw=False):
        self.lmList = []
        if self.result.multi_hand_landmarks:
            myHands = self.result.multi_hand_landmarks[handid]
            for id, lm in enumerate(myHands.landmark):  # 这里输出的是关键点在图像上的比例坐标
                    # print(id, lm)
                    h, w, c = img.shape
                    cx, cy = int(lm.x * w), int(lm.y * h)
                    # print(id, cx, cy)
                    self.lmList.append([id, cx, cy])
                    if is_draw:
                        cv2.circle(img, (cx, cy), 15, (255, 255, 0), cv2.FILLED)
        return self.lmList
    
    def findPositionBox(self, img, handid=0, is_draw=False):
        self.lmList = []
        xlist = []
        ylist = []
        bbox = []
        if self.result.multi_hand_landmarks:
            myHands = self.result.multi_hand_landmarks[handid]
            for id, lm in enumerate(myHands.landmark):  # 这里输出的是关键点在图像上的比例坐标
                    # print(id, lm)
                    h, w, c = img.shape
                    cx, cy = int(lm.x * w), int(lm.y * h)
                    # print(id, cx, cy)
                    self.lmList.append([id, cx, cy])
                    if is_draw:
                        cv2.circle(img, (cx, cy), 5, (0, 255, 0), cv2.FILLED)
                    xlist.append(cx)
                    ylist.append(cy)
            minx, maxx = min(xlist), max(xlist)
            miny, maxy = min(ylist), max(ylist)
            bbox = minx, miny, maxx, maxy
            
            print(bbox)
            if is_draw:
                cv2.rectangle(img, (minx - 20, miny - 29), (maxx + 20, maxy + 20), (0, 255, 0), 3)
                    
        return self.lmList, bbox
    
    def fingerup(self):     # 判断手指是否为展开
        finger = [0, 0, 0, 0, 0]
        if len(self.lmList) != 0:
            finger[0] = int(self.lmList[4][1] < self.lmList[2][1])    # 单独处理大拇指（假设只使用右手，则通过大拇指指尖和根点的横坐标进行判断）
            # angle = math.degrees(math.atan2(lmList[4][2] - lmList[0][2], lmList[4][1] - lmList[0][1]) - 
            #                      math.atan2(lmList[5][2] - lmList[0][2], lmList[5][1] - lmList[0][1]))
            # print(angle)
            for i in range(1, 5):
                finger[i] = int(self.lmList[4 * (i + 1)][2] < self.lmList[2 + i * 4][2])  # 手指的指尖坐标在纵坐标上小于根点的坐标判断（注意cv2的坐标为x轴向右为正，y轴向下为正）
                
        return finger
    
    def fingerdistance(self, img, p1, p2, is_draw=True, r=15, t=3):
        x1, y1 = self.lmList[p1][1:]
        x2, y2 = self.lmList[p2][1:]
        cx, cy = (x1 + x2) // 2, (y1 + y2) // 2
        
        if is_draw:
            cv2.line(img, (x1, y1), (x2, y2), (255, 0, 255), 3)
            cv2.circle(img, (x1, y1), 5, (255, 0, 255), cv2.FILLED)
            cv2.circle(img, (x2, y2), 5, (255, 0, 255), cv2.FILLED)
            cv2.circle(img, (cx, cy), 5, (0, 0, 255), cv2.FILLED)
        length = math.hypot(x2 - x1, y2 - y1)
        
        return length, img, {x1, y1, x2, y2, cx, cy}