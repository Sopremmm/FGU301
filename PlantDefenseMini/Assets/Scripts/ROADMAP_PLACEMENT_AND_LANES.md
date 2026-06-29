# Placement And Lane Targeting Roadmap

## 1. Lane-Based Targeting

Muc tieu: game dung logic theo lane giong Plant vs Zombies. Unit chi target unit dich trong cung hang, khong quet toan scene.

### GridGenerator2D

- GridGenerator2D can expose thong tin lane.
- Luu so hang `rows`.
- Luu world Y cua tung hang.
- Them API du kien:
  - `GetLaneY(int laneIndex)`
  - `GetNearestLaneIndex(Vector3 worldPosition)`
  - `GetCellWorldPosition(int row, int column)`
- Khi sinh grid, moi cell nen biet row/column cua no.

### UnitController

- Them lane cho unit:
  - `LaneIndex`
  - `SetLane(int laneIndex)`
- Unit khong tu doan lane. Lane se duoc set boi placement/spawner.
- UnitController van la trung tam ket noi module:
  - Data
  - Faction
  - CurrentTarget
  - LaneIndex
  - IsDead

### LaneManager

- Quan ly danh sach unit theo lane.
- API du kien:
  - `Register(UnitController unit, int laneIndex)`
  - `Unregister(UnitController unit)`
  - `UpdateUnitLane(UnitController unit, int newLaneIndex)`
  - `GetUnitsInLane(int laneIndex)`
  - `GetEnemiesInLane(UnitController owner)`
- Khi unit chet thi unregister khoi manager.

### TargetFinderModule

- Khong dung `FindObjectsByType` nua.
- Lay `owner.LaneIndex`.
- Hoi LaneManager danh sach unit trong lane do.
- Bo qua:
  - chinh owner
  - unit da chet
  - unit cung faction
  - unit ngoai `attackRange`
- Uu tien target gan nhat.
- Neu target hien tai con song va van trong range/cung lane thi giu target.
- Neu target chet, ngoai range, hoac khac lane thi tim lai.

### EnemySpawner

- Spawn enemy theo lane.
- Chon `laneIndex`.
- Lay world Y tu GridGenerator2D.
- Spawn enemy o ben phai man hinh hoac spawn point cua lane.
- Goi:
  - `unit.SetLane(laneIndex)`
  - `laneManager.Register(unit, laneIndex)`

## 2. Character Purchase And Placement

Muc tieu: nguoi choi bam mua tren UI, tao preview object di theo chuot, sau do dat character vao o grid hop le.

### Character Shop UI

- Moi nut UI dai dien cho mot `CharacterData`.
- Khi bam nut mua:
  - kiem tra tien hien tai co du `CharacterData.Cost` khong.
  - neu du tien thi bat dau placement mode.
  - neu khong du tien thi co the disable nut hoac bao UI sau.

### PlacementManager

- Quan ly trang thai dat character.
- State du kien:
  - khong dat gi
  - dang cam preview character
- API du kien:
  - `StartPlacement(CharacterData data)`
  - `CancelPlacement()`
  - `ConfirmPlacement(GridCell cell)`
- Khi `StartPlacement`:
  - tao preview GameObject tu prefab hoac preview prefab.
  - preview di theo chuot.
  - chua register vao LaneManager.
  - chua tru tien.
- Khi confirm vao cell hop le:
  - spawn character that tai cell.
  - set lane theo `cell.Row`.
  - register vao LaneManager.
  - tru tien.
  - danh dau cell da co unit.
  - xoa preview.

### Placement Preview

- Preview object di theo chuot.
- Nen co visual rieng:
  - trong suot hon
  - doi mau khi cell hop le/khong hop le
- Preview khong nen co combat module hoat dong.
- Co the dung prefab rieng hoac instantiate prefab that roi disable script combat trong luc preview.

### GridCell

- Moi o grid nen co script rieng.
- Data can co:
  - `Row`
  - `Column`
  - `IsOccupied`
  - `OccupyingUnit`
- API du kien:
  - `Init(int row, int column, GridGenerator2D grid)`
  - `CanPlace(CharacterData data)`
  - `Place(UnitController unit)`
  - `Clear()`
- GridCell co the xu ly hover/click neu dung collider.
- Neu khong dung collider, PlacementManager co the raycast tu chuot de tim GridCell.

### Grid Occupancy Rules

- Chi dat character vao cell trong grid.
- Khong dat len cell da co unit.
- Khong dat neu khong du tien.
- Wall/vat chan cung tinh la occupying unit.
- Enemy khong occupy grid cell vi enemy di chuyen theo lane.

### Currency Manager

- Quan ly tien/sun.
- API du kien:
  - `CanAfford(int cost)`
  - `Spend(int cost)`
  - `Add(int amount)`
- UI shop doc currency de enable/disable nut mua.

### Placement Flow

1. Player bam nut mua character tren UI.
2. Shop UI goi `PlacementManager.StartPlacement(characterData)`.
3. PlacementManager tao preview va cho no di theo chuot.
4. Player hover len grid cell.
5. GridCell/PlacementManager check cell co hop le khong.
6. Player click vao cell hop le.
7. Spawn character that tai cell.
8. Set lane bang row cua cell.
9. Register character vao LaneManager.
10. Tru tien.
11. Cell danh dau occupied.
12. Xoa preview va thoat placement mode.

## 3. Suggested Implementation Order

1. Tao `GridCell` va sua `GridGenerator2D` de gan row/column cho tung cell.
2. Tao `CurrencyManager`.
3. Tao `PlacementManager` voi preview di theo chuot.
4. Tao UI shop button goi `StartPlacement(CharacterData data)`.
5. Hoan thien rule dat unit vao cell.
6. Sau khi placement on dinh, quay lai lam `LaneManager`.
7. Sua `UnitController` co `LaneIndex`.
8. Sua `TargetFinderModule` chi target trong cung lane.
9. Tao `EnemySpawner` spawn theo lane.
