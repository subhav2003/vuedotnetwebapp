<template>
  <div class="checkout-page min-h-screen flex flex-col bg-gray-900 text-[#C8B280]">
    <NavBar />
    <Toast :toasts="toasts" @close="removeToast" />

    <main class="flex-1 container mx-auto px-4 py-6 grid grid-cols-1 lg:grid-cols-3 gap-8">
      <!-- Cart Summary -->
      <section class="bg-[#1a1a1a] p-6 rounded-xl shadow border border-[#333] lg:col-span-2">
        <h2 class="text-2xl font-bold text-[#F5DEB3] mb-6 border-b pb-2">Review Your Cart</h2>

        <div v-if="lines.length" class="space-y-6">
          <div
              v-for="item in lines"
              :key="item.id"
              class="flex flex-col md:flex-row gap-4 border-b pb-4"
          >
            <img
                :src="item.images[0] || '/fallback.png'"
                :alt="item.title"
                class="w-full md:w-32 h-32 object-cover rounded"
            />

            <div class="flex-1">
              <h3 class="text-lg font-semibold text-[#F5DEB3]">{{ item.title }}</h3>
              <p class="text-sm text-[#aaa] my-2">{{ item.description }}</p>
              <p class="text-sm text-[#C8B280]">
                Price: <span class="font-bold">${{ item.price.toFixed(2) }}</span>
              </p>
              <p class="text-sm text-[#C8B280]">
                Quantity: <span class="font-bold">{{ item.quantity }}</span>
              </p>
              <p class="text-lg text-[#F5DEB3] font-bold mt-2">
                Subtotal: ${{ (item.price * item.quantity).toFixed(2) }}
              </p>
            </div>
          </div>
        </div>

        <div v-else class="bg-[#222] p-8 text-center rounded-lg">
          <p class="text-[#aaa] mb-4">Your cart is empty.</p>
          <router-link to="/shop" class="inline-block px-6 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition">
            Continue Shopping
          </router-link>
        </div>
      </section>

      <!-- Order Summary -->
      <section class="bg-[#1a1a1a] p-6 rounded-xl shadow border border-[#333] h-fit sticky top-8">
        <h2 class="text-xl font-bold text-[#F5DEB3] mb-4 pb-2 border-b">Order Summary</h2>

        <div class="space-y-3 mb-6 text-[#aaa]">
          <div class="flex justify-between">
            <span>Subtotal</span>
            <span>${{ totalAmount.toFixed(2) }}</span>
          </div>
          <div class="flex justify-between">
            <span>Shipping</span>
            <span>Free</span>
          </div>
          <div class="flex justify-between">
            <span>Tax</span>
            <span>${{ (totalAmount * 0.07).toFixed(2) }}</span>
          </div>
          <div class="flex justify-between font-bold text-lg text-[#F5DEB3] pt-2 border-t">
            <span>Total</span>
            <span>${{ (totalAmount + totalAmount * 0.07).toFixed(2) }}</span>
          </div>
        </div>

        <div class="mb-4">
          <label class="block mb-1 font-medium text-[#C8B280]">Address</label>
          <select v-model="selectedAddress" class="w-full p-2 rounded bg-[#2a2a2a] border border-[#444] text-[#F5DEB3]">
            <option value="">Select an address</option>
            <option value="home">Home</option>
            <option value="work">Work</option>
            <option value="other">Other</option>
          </select>
        </div>

        <div class="mb-6">
          <label class="block mb-1 font-medium text-[#C8B280]">Payment Method</label>
          <div class="space-y-2 text-[#F5DEB3]">
            <label class="flex items-center gap-2">
              <input type="radio" value="credit" v-model="paymentMethod" /> Credit Card
            </label>
            <label class="flex items-center gap-2">
              <input type="radio" value="paypal" v-model="paymentMethod" /> PayPal
            </label>
            <label class="flex items-center gap-2">
              <input type="radio" value="cod" v-model="paymentMethod" /> Cash on Delivery
            </label>
          </div>
        </div>

        <button
            @click="payNow"
            :disabled="!isPayNowEnabled || loading"
            class="w-full px-6 py-3 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition font-medium disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <span v-if="!loading">Pay Now</span>
          <span v-else>Processing...</span>
        </button>
      </section>
    </main>

    <Footer />
  </div>
</template>

<script setup>
import { ref, computed, reactive } from 'vue'
import axios from 'axios'
import { useCart } from '@/store/useCart'
import NavBar from '@/components/NavBar.vue'
import Footer from '@/components/Footer.vue'
import Toast from '@/components/Toast.vue'

const baseUrl = process.env.VUE_APP_API_URL || window.location.origin

const { cartItems, clearCart } = useCart()

const selectedAddress = ref('')
const paymentMethod = ref('')
const loading = ref(false)

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

const lines = computed(() => {
  return cartItems.value.map(item => {
    const product = item.product || item
    const images = Array.isArray(product.images) ? product.images.map(p =>
        p.startsWith('http') ? p : `${baseUrl.replace(/\/$/, '')}${p.startsWith('/') ? '' : '/'}${p}`
    ) : ['/fallback.png']

    return {
      id: product.id,
      title: product.title,
      description: product.description,
      price: product.price,
      quantity: item.quantity,
      images
    }
  })
})

const totalAmount = computed(() =>
    lines.value.reduce((sum, item) => sum + item.price * item.quantity, 0)
)

const isPayNowEnabled = computed(() => !!selectedAddress.value && !!paymentMethod.value)

async function payNow() {
  try {
    loading.value = true
    const res = await axios.post(`${baseUrl}/api/order`, {}, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    })
    showToast(`Order placed! Claim Code: ${res.data.claimCode}`, 'success')
    clearCart()
  } catch (err) {
    console.error(err)
    showToast('Failed to place order. Please try again.', 'error')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
option {
  background-color: #1a1a1a;
  color: #f5deb3;
}
</style>
