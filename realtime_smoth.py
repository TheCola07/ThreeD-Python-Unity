import cv2
# from cvzone.PoseModule import PoseDetector
from PoseDetector import myPose
from myHand import HandDetector
import socket
import numpy as np

# cap = cv2.VideoCapture('WeChat_20230218110458.mp4')
cap = cv2.VideoCapture(0)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5053)      # 定义localhost与端口，当然可以定义其他的host
serverAddressPortleft = ("127.0.0.1", 5052)
serverAddressPortriht = ("127.0.0.1", 5054)

# detector = PoseDetector()
poseDetector = myPose()
handDetector = HandDetector(maxHands=2, detectionCon=0.8, minTrackCon=0.5)
# posList = []    # 保存到txt在unity中读取需要数组列表
jointnumPos = 33
jointnumHand = 21

# 平滑
# kalman filter parameters
KalmanParamQ = 0.001
KalmanParamR = 0.0015
KP = np.zeros((jointnumPos,3),dtype=np.float32)
PP = np.zeros((jointnumPos,3),dtype=np.float32)
XP = np.zeros((jointnumPos,3),dtype=np.float32)
# low pass filter parameters
PrevPose3D = np.zeros((6,jointnumPos,3),dtype=np.float32)

KH = np.zeros((jointnumHand,3),dtype=np.float32)
PH = np.zeros((jointnumHand,3),dtype=np.float32)
XH = np.zeros((jointnumHand,3),dtype=np.float32)
# low pass filter parameters
PrevHand3D = np.zeros((6,jointnumHand,3),dtype=np.float32)

while True:
    success, img = cap.read()
    # img = cv2.resize(img, (480,860))
    
    if not success:
        cap = cv2.VideoCapture('E:\pyLearn\ThreeD-Python-Unity-Chan\cxk.mp4')
        PrevPose3D = np.zeros((6,jointnumPos,3),dtype=np.float32)
        PrevHand3D = np.zeros((6,jointnumHand,3),dtype=np.float32)
        continue
        # break
    img = poseDetector.findPose(img)
    lmList, bboxInfo = poseDetector.findPosition(img)
    hands, img = handDetector.findHands(img, flipType=True)
    
    if bboxInfo:
        lmString = ''
        currdata = np.squeeze(lmList)
        # currdata = currdata[:,1:]
        smooth_kps = np.zeros((jointnumPos,3),dtype=np.float32)
        '''
        kalman filter
        
        '''
        for i in range(jointnumPos):
            KP[i] = (PP[i] + KalmanParamQ) / (PP[i] + KalmanParamQ + KalmanParamR)
            PP[i] = KalmanParamR * (PP[i] + KalmanParamQ) / (PP[i] + KalmanParamQ + KalmanParamR)
        for i in range(jointnumPos):
            smooth_kps[i] = XP[i] + (currdata[i] - XP[i])*KP[i]
            XP[i] = smooth_kps[i]

        # datakalman[idx] = smooth_kps # record kalman result

        '''
        low pass filter
        '''    
        LowPassParam = 0.1
        PrevPose3D[0] = smooth_kps
        for j in range(1,6):
            PrevPose3D[j] = PrevPose3D[j] * LowPassParam + PrevPose3D[j - 1] * (1.0 - LowPassParam)
        # datafinal[idx] = PrevPose3D[5] # record kalman+low pass result.
        print(PrevPose3D[5])
        for lm in PrevPose3D[5]:
            print(lm)
            lmString += f'{lm[0]},{img.shape[0] - lm[1]},{lm[2]},'
        # posList.append(lmString)
    
        date = lmString
        sock.sendto(str.encode(str(date)), serverAddressPort)
        
    if hands:
        for hand in hands:
            lmListHand = hand['lmList']
            lmString = ''
            currdata = np.squeeze(lmListHand)
            # currdata = currdata[:,1:]
            smooth_kps = np.zeros((jointnumHand,3),dtype=np.float32)
            '''
            kalman filter
            
            '''
            for i in range(jointnumHand):
                KH[i] = (PH[i] + KalmanParamQ) / (PH[i] + KalmanParamQ + KalmanParamR)
                PH[i] = KalmanParamR * (PH[i] + KalmanParamQ) / (PH[i] + KalmanParamQ + KalmanParamR)
            for i in range(jointnumHand):
                smooth_kps[i] = XH[i] + (currdata[i] - XH[i])*KH[i]
                XH[i] = smooth_kps[i]

            # datakalman[idx] = smooth_kps # record kalman result

            '''
            low pass filter
            '''    
            LowPassParam = 0.1
            PrevHand3D[0] = smooth_kps
            for j in range(1,6):
                PrevHand3D[j] = PrevHand3D[j] * LowPassParam + PrevHand3D[j - 1] * (1.0 - LowPassParam)
            # datafinal[idx] = PrevPose3D[5] # record kalman+low pass result.
            print(PrevHand3D[5])
            for lm in PrevHand3D[5]:
                print(lm)
                lmString += f'{lm[0]},{img.shape[0] - lm[1]},{lm[2]},'
            # posList.append(lmString)
        
            date = lmString
            if hand['type'] == "Right":
                sock.sendto(str.encode(str(date)), serverAddressPortleft)
            elif hand['type'] == "Left":
                sock.sendto(str.encode(str(date)), serverAddressPortriht)
            
    
    cv2.namedWindow("Image",0)
    cv2.resizeWindow("Image", 720, 480)
    cv2.imshow("Image", img)
    key = cv2.waitKey(1)
    # 记录数据到本地
    # if key == ord('r'):    
    # with open("MotionData.txt", 'w') as f:
    #     f.writelines(["%s\n" % item for item in posList])
    if key == ord('q'):
        break

