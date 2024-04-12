import cv2
import mediapipe as mp
import math
import time
import numpy as np
#import cv2.cuda as cuda

#cv2.setUseOptimized(True)

cap = cv2.VideoCapture(0)
cap.set(3, 1280)
cap.set(4, 960)

class myPose:
    def __init__(self,
                 mode=False,
                 upbody=False,
                 smooth=True,
                 mindetectionCon=0.5,
                 mintrackCon=0.5):
        self.mode = mode
        self.upbody = upbody
        self.smooth = smooth
        self.mindetectionCon = mindetectionCon
        self.mintrackCon = mintrackCon
        
        self.mpPose = mp.solutions.pose
        self.myPose = self.mpPose.Pose(min_detection_confidence=0.5, min_tracking_confidence=0.5)
        self.mpDraw = mp.solutions.drawing_utils 
        
    def findPose(self, img, is_draw=False):
        
        imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        
        self.result = self.myPose.process(imgRGB)
        if self.result.pose_landmarks:
            if is_draw:
                self.mpDraw.draw_landmarks(img, self.result.pose_landmarks, self.mpPose.POSE_CONNECTIONS)

        return img

    
    def findPosition(self, img, is_draw=False, bboxWithHands=False):
        self.lmList = []
        self.bboxInfo = {}
        if self.result.pose_landmarks:
            for id, lm in enumerate(self.result.pose_landmarks.landmark):
                h, w, c = img.shape
                cx, cy, cz = int(lm.x * w), int(lm.y * h), int(lm.z * w)
                self.lmList.append([cx, cy, cz])
                
            # Bounding Box
            ad = abs(self.lmList[12][0] - self.lmList[11][0]) // 2
            if bboxWithHands:
                x1 = self.lmList[16][0] - ad
                x2 = self.lmList[15][0] + ad
            else:
                x1 = self.lmList[12][0] - ad
                x2 = self.lmList[11][0] + ad
                
            y2 = self.lmList[29][1] + ad
            y1 = self.lmList[1][1] - ad
            bbox = (x1, y1, x2 - x1, y2 - y1)
            cx, cy = bbox[0] + (bbox[2] // 2), bbox[1] + bbox[3] // 2
            
            self.bboxInfo = {"bbox": bbox, "center": (cx, cy)}
            
            if is_draw:
                cv2.rectangle(img, bbox, (255, 0, 255), 3)
                cv2.circle(img, (cx, cy), 5, (255, 0, 0), cv2.FILLED)
                
        return self.lmList, self.bboxInfo
    
    def findDistance(self, p1, p2, img=None, color=(255, 0, 255), scale=5):
        x1, y1 = p1
        x2, y2 = p2
        cx, cy = (x1 + x2) // 2, (y1 + y2) // 2
        length = math.hypot(x2 - x1, y2 - y1)
        info = (x1, y1, x2, y2, cx, cy)

        if img is not None:
            cv2.line(img, (x1, y1), (x2, y2), color, max(1, scale // 3))
            cv2.circle(img, (x1, y1), scale, color, cv2.FILLED)
            cv2.circle(img, (x2, y2), scale, color, cv2.FILLED)
            cv2.circle(img, (cx, cy), scale, color, cv2.FILLED)

        return length, img, info
    
    def findDistance3D(self, p1, p2, img=None, color=(255, 0, 255), scale=5):
        x1, y1, z1 = p1
        x2, y2, z2 = p2
        cx, cy, cz = (x1 + x2) // 2, (y1 + y2) // 2, (z1 + z2) // 2
        length = math.hypot(x2 - x1, y2 - y1, z2 - z1) 
        info = (x1, y1, z1, x2, y2, z2, cx, cy, cz)
        
        if img is not None:
            cv2.line(img, (x1, y1), (x2, y2), color, max(1, scale // 3))
            cv2.circle(img, (x1, y1), scale, color, cv2.FILLED)
            cv2.circle(img, (x2, y2), scale, color, cv2.FILLED)
            cv2.circle(img, (cx, cy), scale, color, cv2.FILLED)

        return length, img, info           
                
    def findAngle(self, p1, p2, p3, img=None, color=(255, 0, 255), scale=5):
        x1, y1 = p1
        x2, y2 = p2
        x3, y3 = p3

        # Calculate the Angle
        angle = math.degrees(math.atan2(y3 - y2, x3 - x2) -
                             math.atan2(y1 - y2, x1 - x2))
        if angle < 0:
            angle += 360

        # Draw
        if img is not None:
            cv2.line(img, (x1, y1), (x2, y2), (255, 255, 255), max(1,scale//5))
            cv2.line(img, (x3, y3), (x2, y2), (255, 255, 255), max(1,scale//5))
            cv2.circle(img, (x1, y1), scale, color, cv2.FILLED)
            cv2.circle(img, (x1, y1), scale+5, color, max(1,scale//5))
            cv2.circle(img, (x2, y2), scale, color, cv2.FILLED)
            cv2.circle(img, (x2, y2), scale+5, color, max(1,scale//5))
            cv2.circle(img, (x3, y3), scale, color, cv2.FILLED)
            cv2.circle(img, (x3, y3), scale+5, color, max(1,scale//5))
            cv2.putText(img, str(int(angle)), (x2 - 50, y2 + 50),
                        cv2.FONT_HERSHEY_PLAIN, 2, color, max(1,scale//5))
        return angle, img

    def angleCheck(self, myAngle, targetAngle, offset=20):
        return targetAngle - offset < myAngle < targetAngle + offset
    
    
def main():
    
    mypose = myPose()
    
    pTime = 0
    
    while True:
        success, img = cap.read()
    
        img = mypose.findPose(img)
        
        lmList = mypose.findPosition(img)
        
        if len(lmList) != 0:
            print(lmList)
        
        cTime = time.time()
        fps = 1 / (cTime - pTime)
        pTime = cTime
        
        cv2.putText(img, str(int(fps)), (70, 50), cv2.FONT_HERSHEY_COMPLEX, 3, (255, 0, 0), 3)
        
        cv2.imshow('image', img)
        
        #time.sleep(1)
        
        if cv2.waitKey(1) & 0xFF==27:  #每帧滞留20毫秒后消失，ESC键退出
            break
        
    return    

if __name__ == "__main__":
    main()