<template>
  <div class="flex flex-col min-h-screen bg-gray-900 text-white">
    <NavBar />

    <main class="flex-1 max-w-6xl mx-auto py-8 px-6 grid grid-cols-1 lg:grid-cols-3 gap-8">
      <!-- Cart Summary -->
      <section class="bg-white/90 text-black p-6 rounded-lg shadow-lg lg:col-span-2">
        <h2 class="text-2xl font-bold text-brown-700 mb-6 border-b pb-2">Review Your Cart</h2>

        <div v-if="lines.length" class="space-y-6">
          <div v-for="product in lines" :key="product.id" class="flex flex-col md:flex-row gap-4 border-b pb-4">
            <div class="w-full md:w-1/4">
              <img
                  :src="normalizeImage(product.image)"
                  :alt="product.title"
                  class="rounded-md shadow-sm object-cover w-full aspect-square"
              />
            </div>
            <div class="flex-1">
              <h3 class="text-lg font-semibold text-brown-700">{{ product.title }}</h3>
              <p class="text-gray-600 mt-1">{{ product.description || 'No description available' }}</p>
              <div class="flex justify-between items-end mt-4">
                <div>
                  <p class="flex items-center gap-2">
                    <span class="text-gray-500">Price:</span>
                    <span class="font-medium">${{ product.price.toFixed(2) }}</span>
                  </p>
                  <p class="flex items-center gap-2">
                    <span class="text-gray-500">Quantity:</span>
                    <span class="font-medium">{{ product.quantity }}</span>
                  </p>
                </div>
                <p class="text-lg font-bold text-brown-700">
                  ${{ (product.price * product.quantity).toFixed(2) }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <div v-else class="bg-gray-100 p-8 text-center rounded-lg">
          <p class="text-gray-500">Your cart is empty.</p>
          <router-link to="/shop" class="mt-4 inline-block px-6 py-2 bg-brown-600 text-white rounded hover:bg-brown-700 transition">
            Continue Shopping
          </router-link>
        </div>
      </section>

      <!-- Order Summary -->
      <section class="bg-white/90 text-black p-6 rounded-lg shadow-lg h-fit sticky top-8">
        <h2 class="text-xl font-bold text-brown-700 mb-4 pb-2 border-b">Order Summary</h2>

        <div class="space-y-3 mb-6">
          <div class="flex justify-between">
            <span class="text-gray-600">Subtotal</span>
            <span>${{ totalAmount.toFixed(2) }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-600">Shipping</span>
            <span>Free</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-600">Tax</span>
            <span>${{ (totalAmount * 0.07).toFixed(2) }}</span>
          </div>
          <div class="flex justify-between font-bold text-lg pt-2 border-t">
            <span>Total</span>
            <span>${{ (totalAmount + totalAmount * 0.07).toFixed(2) }}</span>
          </div>
        </div>

        <!-- Address Selection -->
        <div class="mb-6">
          <label class="block text-gray-700 mb-2 font-medium">Select Shipping Address</label>
          <select
              v-model="selectedAddress"
              class="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-brown-300"
          >
            <option value="">Select an address</option>
            <option value="home">Home</option>
            <option value="work">Work</option>
            <option value="other">Other</option>
          </select>
        </div>

        <!-- Payment Method Selection -->
        <div class="mb-6">
          <label class="block text-gray-700 mb-2 font-medium">Select Payment Method</label>
          <div class="space-y-2">
            <label class="flex items-center gap-2">
              <input type="radio" value="credit" v-model="paymentMethod" />
              <span>Credit Card</span>
            </label>
            <label class="flex items-center gap-2">
              <input type="radio" value="paypal" v-model="paymentMethod" />
              <span>PayPal</span>
            </label>
            <label class="flex items-center gap-2">
              <input type="radio" value="cod" v-model="paymentMethod" />
              <span>Cash on Delivery</span>
            </label>
          </div>
        </div>

        <!-- Pay Now -->
        <button
            @click="payNow"
            :disabled="!isPayNowEnabled"
            class="w-full px-6 py-3 bg-brown-600 text-white rounded-md hover:bg-brown-700 transition font-medium disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Pay Now
        </button>
      </section>
    </main>

    <Footer />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useCart } from '@/store/useCart'
import NavBar from '@/components/NavBar.vue'
import Footer from '@/components/Footer.vue'

const { cartItems } = useCart()
const baseUrl = process.env.VUE_APP_API_URL || window.location.origin

function normalizeImage(path) {
  if (!path) return '/fallback.png'
  if (/^https?:\/\//.test(path)) return path
  return `${baseUrl.replace(/\/$/, '')}${path.startsWith('/') ? '' : '/'}${path}`
}

const lines = computed(() =>
    cartItems.value.map(item => ({
      id: item.product.id,
      title: item.product.title,
      price: item.product.price,
      quantity: item.quantity,
      image: Array.isArray(item.product.images) && item.product.images.length ? item.product.images[0] : '',
      description: item.product.description
    }))
)

const totalAmount = computed(() =>
    lines.value.reduce((sum, item) => sum + item.price * item.quantity, 0)
)

const selectedAddress = ref('')
const paymentMethod = ref('')
const isPayNowEnabled = computed(() => !!selectedAddress.value && !!paymentMethod.value)

async function payNow() {
  alert(`Payment initiated using ${paymentMethod.value} at ${selectedAddress.value} address`)
}
</script>

<style scoped>
.bg-brown-600 {
  background-color: #a0522d;
}
.bg-brown-700 {
  background-color: #8b4513;
}
.text-brown-700 {
  color: #8b4513;
}
.ring-brown-300 {
  --tw-ring-color: rgba(160, 82, 45, 0.3);
}
</style>
