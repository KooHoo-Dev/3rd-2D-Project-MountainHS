# **👻 Sprite Matting Tool v3.1**

**Pixel-Perfect Sprite & Tileset Processor for Unity Developers**
> 생성형 AI의 왜곡(Hallucination) 없이, 알고리즘 기반으로 정교하게 배경을 제거하고 스프라이트를 추출하며 타일셋으로 내보내는 웹 기반 도구입니다.

## **📌 Overview**

**Sprite Matting Tool**은 픽셀 아트와 스프라이트 시트 작업에 최적화된 경량화 웹 어플리케이션입니다.

복잡한 설치 없이 브라우저에서 즉시 실행되며, **Flood Fill(Smart Eraser)** 알고리즘을 사용하여 캐릭터 내부의 색상을 보존하면서 배경만 완벽하게 제거할 수 있습니다. v3.x 버전에서는 단순 Crop을 넘어, 불투명 영역을 자동으로 인식해 여백 없이 잘라내는 **Auto-Trim Extract**와 시트 전체를 격자로 분할하는 **Grid Slicer**, 그리고 추출한 조각을 한 번에 내보내는 **Tile Tray & ZIP Export**가 추가되어 본격적인 타일셋 제작 워크플로우를 지원합니다.

### **🎯 Core Philosophy**

* **Deterministic:** 확률적인 AI 추론 대신, 정확한 픽셀 연산을 통해 원본의 형태를 100% 보존합니다.
* **Interactive:** 실시간 캔버스 조작(Zoom/Pan)과 직관적인 클릭/드래그 인터페이스를 제공합니다.
* **Standalone:** 서버 업로드 없이 클라이언트 브라우저에서 모든 처리가 완료되어 보안에 안전합니다.

## **✨ Key Features (v3.1)**

| Feature | Description |
| :---- | :---- |
| **🌪️ Smart Eraser** | **Flood Fill 알고리즘**을 사용하여 클릭한 지점과 연결된 배경색만 지능적으로 제거합니다. (캐릭터 내부 보호) |
| **🪄 Color Key** | 클릭한 색상을 이미지 **전역**에서 일괄 제거합니다. 단색 배경(예: #FF00FF)을 한 번에 빼낼 때 유용합니다. |
| **🔲 Auto-Trim Extract** | 드래그한 영역 안에서 **불투명 픽셀의 경계까지 박스를 자동 정렬**하여 여백 없이 추출합니다. 8방향 핸들로 픽셀 단위 미세 조정이 가능합니다. |
| **🧩 Grid Slicer** | 시트 전체를 **셀 크기 또는 행/열 개수**로 격자 분할합니다. Offset/Spacing 지원, 실시간 격자 프리뷰, 각 셀 Auto-Trim 및 빈 셀 자동 제외. |
| **🗂️ Tile Tray & ZIP Export** | 추출한 타일을 우측 트레이에 모아 개별 PNG 다운로드, 또는 **ZIP 일괄 내보내기**가 가능합니다. |
| **↩️ Undo System** | 실수해도 걱정 없습니다. Ctrl + Z를 통해 작업 내역을 되돌릴 수 있습니다. (최대 20단계) |
| **🎚️ Tolerance Control** | JPG 압축 노이즈나 유사 색상 처리를 위해 허용 오차(Tolerance)를 실시간으로 조절할 수 있습니다. |
| **✨ Refinement** | **Despeckle**(미세 노이즈 제거) 및 **Smooth Edges**(외곽선 부드럽게) 필터를 제공합니다. |
| **🎚️ UpScaling** | Sprite의 낮은 해상도를 개선할 수 있는 x4 스케일업 기능. FHD -> 4K 변환 기능을 제공합니다. |

## **🚀 Getting Started**

이 프로젝트는 단일 HTML 파일로 구성되어 있어 별도의 빌드나 설치 과정이 필요 없습니다.

### **Installation**

1. 이 저장소를 클론하거나 SpriteTool_v3.html 파일을 다운로드합니다.
2. 파일을 **Chrome, Edge, Safari** 등 최신 웹 브라우저로 엽니다.
3. 즉시 사용 가능합니다.

## **📖 User Guide**

### **1\. Navigation & Controls**

작업 공간을 효율적으로 제어하기 위한 단축키와 조작법입니다.

* **Zoom:** 마우스 휠 스크롤 (커서 위치 기준 Smart Zoom)
* **Pan (이동):** Middle Mouse Button 드래그 또는 Shift + Click 드래그
* **Undo (실행 취소):** <kbd>Ctrl</kbd> + <kbd>Z</kbd> (Mac: <kbd>Cmd</kbd> + <kbd>Z</kbd>)
* **Add Tile (Extract):** <kbd>Enter</kbd> · **Cancel Box:** <kbd>Esc</kbd>
* **Reset View:** 상단 툴바의 Reset View 버튼 클릭

### **2\. Workflow A: 배경 제거 (Eraser / Color Key)**

스프라이트 시트의 배경을 투명하게 만듭니다.

1. **Upload:** 파일을 드래그 앤 드롭하여 로드합니다.
2. **Select Tool:** 좌측 툴바에서 **Smart Eraser** 또는 **Color Key** 모드를 선택합니다.
3. **Adjust Tolerance:** 배경색이 균일하지 않다면(JPG 등) Tolerance 슬라이더를 조절합니다.
4. **Click:** 캔버스에서 제거할 배경색을 클릭합니다.
   * *Smart Eraser:* 클릭 지점과 **연결된** 영역만 제거됩니다. 닫힌 영역(캐릭터 팔 사이 등)은 별도로 클릭해야 합니다.
   * *Color Key:* 클릭한 색과 같은 색을 **전역**에서 한 번에 제거합니다.
5. **Refine:** 필요시 Despeckle 버튼으로 자잘한 노이즈를 정리합니다.

### **3\. Workflow B: 개별 스프라이트 추출 (Extract \- Single)**

시트에서 필요한 부분만 여백 없이 정교하게 잘라 트레이에 모읍니다.

1. **Select Tool:** 좌측 툴바에서 **Extract** 모드를 선택하고 상단 **Single** 탭을 고릅니다.
2. **Drag:** 대상 스프라이트를 대충 감싸도록 드래그합니다.
3. **Auto-Trim:** **Auto-trim on drag**가 켜져 있으면 불투명 픽셀의 경계까지 박스가 자동으로 줄어듭니다. (Alpha Threshold로 여백 판정 기준 조절)
4. **Adjust:** 8개 핸들로 미세 조정하거나, 박스 안쪽을 끌어 통째로 이동합니다. (Re-trim으로 재정렬)
5. **Add:** **Add** 버튼 또는 <kbd>Enter</kbd>로 타일을 트레이에 추가합니다.

### **4\. Workflow C: 격자 일괄 분할 (Extract \- Grid)**

스프라이트 시트를 일정한 격자로 나눠 모든 칸을 한 번에 추출합니다.

1. **Select Tool:** **Extract** 모드에서 상단 **Grid** 탭을 선택합니다.
2. **Set Grid:** 분할 기준을 **Cell Size**(셀 W/H 픽셀) 또는 **Cell Count**(행/열 개수) 중에서 입력합니다.
3. **Align:** 필요시 **Offset**(시작 여백)과 **Spacing**(셀 간격)을 조절합니다. 캔버스의 보라색 격자 프리뷰가 실시간으로 갱신됩니다.
4. **Trim Option:** **Auto-trim each cell & skip empty**(기본 켜짐)로 각 셀을 자동 정렬하고 빈 칸은 건너뜁니다.
5. **Slice:** **Slice & Add All** 버튼으로 모든 칸을 트레이에 한 번에 담습니다.

### **5\. Workflow D: 내보내기 (Tile Tray & Export)**

추출한 타일을 저장합니다.

1. **Review:** 우측 **Extracted Tiles** 트레이에서 썸네일과 크기를 확인합니다.
2. **Manage:** 타일 카드의 다운로드 아이콘(개별 PNG), X 아이콘(제거), 상단 Clear(전체 비우기)로 관리합니다.
3. **Export:** 하단 **Download All (ZIP)**으로 모든 타일을 ZIP 한 개로 일괄 저장합니다.
   * 파일명은 `원본이름_001_64x64.png` 형식으로 번호와 크기가 자동 부여됩니다.
4. **Save Sheet:** 편집한 시트 전체가 필요하면 툴바 하단 **Save Current Sheet**로 PNG 저장합니다.

## **🛠️ Technical Stack**

* **Core:** Vanilla JavaScript (ES6+), HTML5 Canvas API
* **Export:** JSZip (CDN) — 타일셋 ZIP 일괄 내보내기
* **Styling:** Tailwind CSS (CDN)
* **Icons:** FontAwesome (CDN)
* **Font:** JetBrains Mono (Code/Numbers), Noto Sans KR (UI)

## **📝 License**

This project is licensed under the MIT License - see the [LICENSE](https://www.google.com/search?q=LICENSE) file for details.

**Developed by Jay (KooHoo)**
*Unity Developer & Tech Lead*
