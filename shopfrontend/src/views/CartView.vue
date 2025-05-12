<template>
  <div class="cart-page min-h-screen flex flex-col bg-gray-900 text-[#C8B280]">
    <NavBar />
    <Toast :toasts="toasts" @close="removeToast" />

    <main class="flex-1 container mx-auto px-4 py-6">
      <h1 class="text-3xl font-bold text-[#F5DEB3] mb-6">Your Cart</h1>

      <!-- Empty state -->
      <div v-if="!lines.length" class="flex flex-col items-center justify-center text-[#aaa] py-20">
        <p class="text-lg mb-4">Your cart is currently empty.</p>
        <RouterLink
            to="/shop"
            class="px-6 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition"
        >Continue Shopping</RouterLink>
      </div>

      <!-- Cart items -->
      <div v-else class="space-y-6">
        <transition-group name="fade" tag="div" class="space-y-6">
          <div
              v-for="item in lines"
              :key="item.id"
              class="bg-[#1a1a1a] rounded-xl p-5 flex flex-col md:flex-row items-center shadow border border-[#333] hover:shadow-md transition"
          >
            <img
                :src="item.images[0] || '/fallback.png'"
                :alt="item.title"
                class="w-full md:w-32 h-32 object-cover rounded mb-4 md:mb-0"
            />

            <div class="flex-1 md:px-6 w-full text-left">
              <h2 class="text-xl font-semibold truncate text-[#F5DEB3]">{{ item.title }}</h2>
              <p class="text-sm text-[#aaa] line-clamp-2 my-2">{{ item.description }}</p>
              <p class="text-sm text-[#C8B280]">
                Price: <span class="font-bold">${{ item.price.toFixed(2) }}</span>
              </p>
            </div>

            <div class="flex items-center space-x-2 mt-4 md:mt-0">
              <button
                  @click="changeQty(item, item.quantity - 1)"
                  :disabled="item.quantity <= 1"
                  class="px-3 py-1 bg-[#444] rounded hover:bg-[#555] disabled:opacity-50"
              >–</button>
              <input
                  type="number"
                  :value="item.quantity"
                  @change="manualQty(item, $event)"
                  class="w-14 text-center bg-[#222] text-[#C8B280] border border-[#444] rounded"
              />
              <button
                  @click="changeQty(item, item.quantity + 1)"
                  class="px-3 py-1 bg-[#444] rounded hover:bg-[#555]"
              >+</button>
            </div>

            <div class="text-right mt-4 md:mt-0 ml-6">
              <p class="text-[#C8B280]">
                Subtotal: <span class="font-bold">${{ (item.price * item.quantity).toFixed(2) }}</span>
              </p>
              <button
                  @click="removeItem(item)"
                  class="text-red-400 hover:text-red-500 text-sm mt-2"
              >Remove</button>
            </div>
          </div>
        </transition-group>

        <!-- Summary -->
        <div class="bg-[#1a1a1a] p-6 rounded-xl shadow border border-[#333] flex flex-col md:flex-row justify-between items-center">
          <div class="text-lg">
            <p>Total Items: <span class="font-semibold">{{ totalItems }}</span></p>
            <p>Total Price: <span class="text-[#F5DEB3] font-bold">${{ totalAmount.toFixed(2) }}</span></p>
          </div>
          <div class="mt-4 md:mt-0 space-x-4">
            <button
                @click="clearCartItems"
                class="px-6 py-2 bg-[#8B0000] text-white rounded hover:bg-[#a30000] transition"
            >Clear Cart</button>
            <RouterLink
                to="/checkout"
                class="px-6 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition"
            >Checkout</RouterLink>
          </div>
        </div>
      </div>
    </main>

    <Footer />
  </div>
</template>

<script setup>
import { computed, reactive } from 'vue'
import { useCart }       from '@/store/useCart'
import NavBar            from '@/components/NavBar.vue'
import Footer            from '@/components/Footer.vue'
import Toast             from '@/components/Toast.vue'
import { RouterLink }    from 'vue-router'

// Base URL from webpack env
const baseUrl = process.env.VUE_APP_API_URL || window.location.origin

// Cart store (no fetchCart)
const { cartItems, updateQuantity, removeFromCart, clearCart } = useCart()

// unwrap API shapes into array
const rawItems = computed(() => {
  const v = cartItems.value
  if (Array.isArray(v)) return v
  if (v?.cart?.products) return v.cart.products
  console.warn('CartView: unexpected cartItems shape', v)
  return []
})

// normalize into flat shape
const lines = computed(() => {
  try {
    return rawItems.value.map(i => {
      let id, title, description, price, quantity, rawImgs
      if ('bookId' in i) {
        id = i.bookId; title = i.title; description = i.description;
        price = i.price; quantity = i.quantity; rawImgs = i.images
      } else if (i.product?.bookId !== undefined) {
        id = i.product.bookId; title = i.product.title || i.product.name;
        description = i.product.description; price = i.product.price;
        quantity = i.quantity; rawImgs = i.product.images
      } else if (i.product?.id !== undefined) {
        id = i.product.id; title = i.product.name;
        description = i.product.description; price = i.product.price;
        quantity = i.quantity; rawImgs = i.product.images
      } else {
        console.error('CartView: unknown item shape', i)
        return null
      }
      const images = (rawImgs || []).map(p =>
          p.startsWith('http') ? p : `${baseUrl.replace(/\/$/, '')}${p.startsWith('/') ? '' : '/'}${p}`
      )
      return { id, title, description, price, quantity, images }
    }).filter(Boolean)
  } catch (err) {
    console.error('CartView normalization error:', err)
    return []
  }
})

// totals
const totalItems  = computed(() => lines.value.reduce((sum, x) => sum + x.quantity, 0))
const totalAmount = computed(() => lines.value.reduce((sum, x) => sum + x.price * x.quantity, 0))

// toasts
const toasts = reactive([])
const removeToast = id => {
  const idx = toasts.findIndex(t => t.id === id)
  if (idx > -1) toasts.splice(idx, 1)
}
const showToast = (msg, type = 'success') => {
  const id = Date.now() + Math.random()
  toasts.push({ id, message: msg, type })
  setTimeout(() => removeToast(id), 5000)
}

// quantity update
async function changeQty(item, desiredQty) {
  if (desiredQty < 1) return
  const prev = item.quantity
  try {
    const { success, message } = await updateQuantity(item.id, desiredQty)
    if (!success) throw new Error(message)
    item.quantity = desiredQty
    showToast(message)
  } catch (err) {
    console.error(`CartView: failed to update qty for ${item.id}`, err)
    item.quantity = prev
    showToast(err.message || 'Failed to update quantity', 'error')
  }
}

// manual input
async function manualQty(item, e) {
  const val = parseInt(e.target.value, 10)
  if (!val || val < 1) { e.target.value = item.quantity; return }
  await changeQty(item, val)
  e.target.value = item.quantity
}

// remove
async function removeItem(item) {
  try {
    const { success, message } = await removeFromCart(item.id)
    showToast(message, success ? 'success' : 'error')
  } catch (err) {
    console.error(`CartView: failed to remove ${item.id}`, err)
    showToast('Failed to remove item', 'error')
  }
}

// clear
async function clearCartItems() {
  try {
    const { success, message } = await clearCart()
    showToast(message, success ? 'success' : 'error')
  } catch (err) {
    console.error('CartView: failed to clear cart', err)
    showToast('Failed to clear cart', 'error')
  }
}
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.fade-enter-active, .fade-leave-active {
  transition: all 0.3s ease;
}
.fade-enter-from {
  opacity: 0; transform: translateY(10px);
}
.fade-enter-to {
  opacity: 1; transform: translateY(0);
}
.fade-leave-to {
  opacity: 0; transform: translateY(10px);
}
</style>