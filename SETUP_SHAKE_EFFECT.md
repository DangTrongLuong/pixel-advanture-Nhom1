# 🔥 Setup Camera Shake + Wall Shake cho Map 12 & Map 13

## ✅ Scripts đã tạo:

- `Assets/Scripts/Utils/CameraShake.cs` - Rung camera
- `Assets/Scripts/Utils/ShakeObject.cs` - Rung object (tường)
- **Sửa `RockHeadVertical.cs` & `SpikeHeadVertical.cs`** - Gọi shake khi va chạm

---

## 📋 Setup trong Unity Editor

### **Bước 1️⃣: Setup CameraShake cho Map 12**

1. Mở **Map12.unity** scene
2. Tìm **Main Camera** trong Hierarchy
3. Thêm component **CameraShake** vào camera này:
   - Click **Add Component** → tìm `CameraShake` → Add ✓
4. **Không cần config gì cả** (script tự quản lý singleton)

✅ **Xong!** CameraShake sẽ tự find bằng `CameraShake.instance`

---

### **Bước 2️⃣: Thêm ShakeObject vào tường - Map 12**

1. Tìm **tilemap/wall container** trong Hierarchy (tường chính)
   - VD: "Walls" hoặc "Tilemap"
2. Thêm component **ShakeObject** vào nó:
   - Click **Add Component** → tìm `ShakeObject` → Add ✓
3. Done! ✅ Bây giờ tường sẽ rung khi RockHead/SpikeHead đập vào

---

### **Bước 3️⃣: Lặp lại cho Map 13**

Làm **hoàn toàn giống** như Bước 1-2 nhưng trong **Map13.unity** scene

---

## 🎮 Tuning Settings (tuỳ chọn)

Nếu muốn **điều chỉnh mục độ rung**, mở file script và thay đổi:

### `RockHeadVertical.cs` - dòng ~156:

```csharp
// Camera shake
StartCoroutine(CameraShake.instance.Shake(0.15f, 0.15f));
//                                          duration  magnitude
//                                          ↓         ↓
// Tăng duration: cảm giác rung lâu hơn
// Tăng magnitude: rung mạnh hơn

// Wall shake
StartCoroutine(wallShake.Shake(0.2f, 0.1f));
//                            duration magnitude
```

### Đề xuất settings:

| Loại                  | Duration | Magnitude | Cảm giác      |
| --------------------- | -------- | --------- | ------------- |
| **Nhẹ (Spike)**       | 0.1f     | 0.08f     | Bó gọn, nhanh |
| **Vừa (RockHead)**    | 0.15f    | 0.1f      | Cân bằng      |
| **Mạnh (Heavy trap)** | 0.2f     | 0.15f     | Impact lớn    |

---

## 🎯 Kiểm tra hoạt động

1. Play scene Map 12
2. Để trap (RockHead/SpikeHead) đập vào tường
3. Nên thấy:
   - ✓ Camera rung (toàn bộ view rung nhẹ)
   - ✓ Tường rung (object tường di chuyển)
   - ✓ Cảm giác "impact" mạnh

Nếu **không có hiệu ứng**:

- Check xem **CameraShake component** đã thêm vào Main Camera chưa?
- Check xem **ShakeObject component** đã thêm vào tường chưa?
- Check Console log có lỗi không?

---

## ✨ Advanced: Sound + Particle (tuỳ chọn)

Nếu muốn thêm sound/particle khi va chạm, sửa trong `RockHeadVertical.cs` sau dòng shake effect:

```csharp
// Thêm sound
AudioSource.PlayClipAtPoint(hitSound, transform.position);

// Thêm particle
Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
```

---

## 📝 Files đã sửa:

- ✅ `SpikeHeadVertical.cs` - Thêm shake vào Bounce()
- ✅ `RockHeadVertical.cs` - Thêm shake vào HitObstacle()
- ✅ `CameraShake.cs` - Script mới
- ✅ `ShakeObject.cs` - Script mới

**Enjoy trải nghiệm "xịn xò" Pixel Adventure! 🎮**
