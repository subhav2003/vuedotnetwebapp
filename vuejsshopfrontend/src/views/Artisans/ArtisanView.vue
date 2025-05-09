<!-- StaffDashboard.vue -->
<template>
  <div class="min-h-screen bg-gray-100 pt-16">
    <!-- Navigation Bar -->
    <artisan-nav-bar/>

    <!-- Main Content -->
    <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <!-- Page Header -->
      <div class="px-4 py-6 sm:px-0">
        <h1 class="text-2xl font-bold text-gray-900">Staff Dashboard</h1>
        <p class="mt-1 text-sm text-gray-600">Manage orders and fulfill customer requests</p>
      </div>

      <!-- Dashboard Widgets -->
      <div class="px-4 py-6 sm:px-0">
        <!-- Stats Overview -->
        <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
          <div class="bg-white overflow-hidden shadow rounded-lg">
            <div class="px-4 py-5 sm:p-6">
              <dt class="text-sm font-medium text-gray-500 truncate">Pending Orders</dt>
              <dd class="mt-1 text-3xl font-semibold text-gray-900">12</dd>
            </div>
          </div>
          <div class="bg-white overflow-hidden shadow rounded-lg">
            <div class="px-4 py-5 sm:p-6">
              <dt class="text-sm font-medium text-gray-500 truncate">Orders Fulfilled Today</dt>
              <dd class="mt-1 text-3xl font-semibold text-gray-900">8</dd>
            </div>
          </div>
          <div class="bg-white overflow-hidden shadow rounded-lg">
            <div class="px-4 py-5 sm:p-6">
              <dt class="text-sm font-medium text-gray-500 truncate">Low Stock Items</dt>
              <dd class="mt-1 text-3xl font-semibold text-gray-900">5</dd>
            </div>
          </div>
          <div class="bg-white overflow-hidden shadow rounded-lg">
            <div class="px-4 py-5 sm:p-6">
              <dt class="text-sm font-medium text-gray-500 truncate">Total Revenue Today</dt>
              <dd class="mt-1 text-3xl font-semibold text-gray-900">$325.95</dd>
            </div>
          </div>
        </div>

        <!-- Order Processing Section -->
        <div class="bg-white shadow overflow-hidden sm:rounded-lg mb-6">
          <div class="px-4 py-5 sm:px-6">
            <h2 class="text-lg leading-6 font-medium text-gray-900">Process Customer Order</h2>
            <p class="mt-1 max-w-2xl text-sm text-gray-500">Enter the claim code provided by the customer.</p>
          </div>

          <div class="border-t border-gray-200 px-4 py-5 sm:px-6">
            <!-- Claim Code Form -->
            <div class="mb-6">
              <div class="flex items-center">
                <div class="w-full sm:w-2/3 md:w-1/2">
                  <div class="mt-1 flex rounded-md shadow-sm">
                    <input
                        type="text"
                        v-model="claimCode"
                        class="focus:ring-indigo-500 focus:border-indigo-500 flex-1 block w-full rounded-md sm:text-sm border-gray-300 px-3 py-2 border"
                        placeholder="Enter claim code"
                    />
                    <button
                        @click="fetchOrderDetails"
                        class="ml-3 inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                    >
                      Verify
                    </button>
                  </div>
                  <p v-if="formError" class="mt-2 text-sm text-red-600">{{ formError }}</p>
                </div>
              </div>
            </div>

            <!-- Order Details (conditionally displayed) -->
            <div v-if="order" class="mt-8">
              <h3 class="text-lg font-medium text-gray-900">Order Details</h3>
              <div class="mt-4 border border-gray-200 rounded-md p-4">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <p class="text-sm font-medium text-gray-500">Order Number</p>
                    <p class="mt-1 text-sm text-gray-900">{{ order.orderNumber }}</p>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500">Date Placed</p>
                    <p class="mt-1 text-sm text-gray-900">{{ order.orderDate }}</p>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500">Customer</p>
                    <p class="mt-1 text-sm text-gray-900">{{ order.customerName }}</p>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500">Membership ID</p>
                    <p class="mt-1 text-sm text-gray-900">{{ order.membershipId }}</p>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500">Total Items</p>
                    <p class="mt-1 text-sm text-gray-900">{{ order.items.length }}</p>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-500">Order Status</p>
                    <span class="mt-1 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium" :class="{
                      'bg-yellow-100 text-yellow-800': order.status === 'Pending',
                      'bg-green-100 text-green-800': order.status === 'Completed',
                      'bg-red-100 text-red-800': order.status === 'Cancelled'
                    }">
                      {{ order.status }}
                    </span>
                  </div>
                </div>

                <!-- Order Items Table -->
                <div class="mt-6">
                  <h4 class="text-sm font-medium text-gray-900">Order Items</h4>
                  <div class="mt-2 flex flex-col">
                    <div class="-my-2 overflow-x-auto sm:-mx-6 lg:-mx-8">
                      <div class="py-2 align-middle inline-block min-w-full sm:px-6 lg:px-8">
                        <div class="shadow overflow-hidden border-b border-gray-200 sm:rounded-lg">
                          <table class="min-w-full divide-y divide-gray-200">
                            <thead class="bg-gray-50">
                            <tr>
                              <th scope="col"
                                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Book Title
                              </th>
                              <th scope="col"
                                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Author
                              </th>
                              <th scope="col"
                                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                ISBN
                              </th>
                              <th scope="col"
                                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Price
                              </th>
                              <th scope="col"
                                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                Format
                              </th>
                            </tr>
                            </thead>
                            <tbody class="bg-white divide-y divide-gray-200">
                            <tr v-for="(item, index) in order.items" :key="index">
                              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{
                                  item.title
                                }}
                              </td>
                              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ item.author }}</td>
                              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ item.isbn }}</td>
                              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${{
                                  item.price.toFixed(2)
                                }}
                              </td>
                              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ item.format }}</td>
                            </tr>
                            </tbody>
                          </table>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Order Summary -->
                <div class="mt-6 bg-gray-50 p-4 rounded-md">
                  <div class="flex justify-between">
                    <span class="text-sm text-gray-500">Subtotal</span>
                    <span class="text-sm font-medium text-gray-900">${{ calculateSubtotal().toFixed(2) }}</span>
                  </div>
                  <div v-if="order.discount > 0" class="flex justify-between mt-2">
                    <span class="text-sm text-gray-500">Discount ({{ order.discount }}%)</span>
                    <span class="text-sm font-medium text-green-600">-${{ calculateDiscount().toFixed(2) }}</span>
                  </div>
                  <div class="flex justify-between border-t border-gray-200 mt-4 pt-4">
                    <span class="text-base font-medium text-gray-900">Total</span>
                    <span class="text-base font-medium text-gray-900">${{ calculateTotal().toFixed(2) }}</span>
                  </div>
                </div>

                <!-- Action Buttons -->
                <div class="mt-6 flex justify-end">
                  <button
                      v-if="order.status === 'Pending'"
                      @click="fulfillOrder"
                      class="ml-3 inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
                  >
                    Complete Order Fulfillment
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Recent Orders -->
        <div class="bg-white shadow overflow-hidden sm:rounded-lg">
          <div class="px-4 py-5 sm:px-6 flex justify-between items-center">
            <div>
              <h2 class="text-lg leading-6 font-medium text-gray-900">Recent Orders</h2>
              <p class="mt-1 max-w-2xl text-sm text-gray-500">Latest orders that need processing.</p>
            </div>
            <div>
              <a href="#" class="text-sm font-medium text-indigo-600 hover:text-indigo-500">View all orders</a>
            </div>
          </div>
          <div class="border-t border-gray-200">
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                <tr>
                  <th scope="col"
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Order #
                  </th>
                  <th scope="col"
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Customer
                  </th>
                  <th scope="col"
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date
                  </th>
                  <th scope="col"
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Items
                  </th>
                  <th scope="col"
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Total
                  </th>
                  <th scope="col"
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status
                  </th>
                  <th scope="col"
                      class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Action
                  </th>
                </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="(recentOrder, index) in recentOrders" :key="index">
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-indigo-600">{{
                      recentOrder.orderNumber
                    }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ recentOrder.customerName }}
                    <div class="text-xs text-gray-400">{{ recentOrder.membershipId }}</div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ recentOrder.orderDate }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ recentOrder.itemCount }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${{ recentOrder.total.toFixed(2) }}</td>
                  <td class="px-6 py-4 whitespace-nowrap">
                      <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full" :class="{
                        'bg-yellow-100 text-yellow-800': recentOrder.status === 'Pending',
                        'bg-green-100 text-green-800': recentOrder.status === 'Completed',
                        'bg-red-100 text-red-800': recentOrder.status === 'Cancelled'
                      }">
                        {{ recentOrder.status }}
                      </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <a href="#" class="text-indigo-600 hover:text-indigo-900 mr-3">View</a>
                    <a v-if="recentOrder.status === 'Pending'" href="#" class="text-green-600 hover:text-green-900">Process</a>
                  </td>
                </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import ArtisanNavBar from "@/components/ArtisansDashboard/ArtisanNavBar.vue";

export default {
  name: 'StaffDashboard',
  components: {ArtisanNavBar},
  data() {
    return {
      claimCode: '',
      formError: '',
      order: null,
      recentOrders: [
        {
          orderNumber: '1093',
          customerName: 'John Doe',
          membershipId: 'M10078',
          orderDate: 'May 09, 2025, 09:45 AM',
          itemCount: 5,
          total: 62.95,
          status: 'Pending'
        },
        {
          orderNumber: '1092',
          customerName: 'Alice Smith',
          membershipId: 'M10063',
          orderDate: 'May 09, 2025, 09:22 AM',
          itemCount: 2,
          total: 34.98,
          status: 'Pending'
        },
        {
          orderNumber: '1091',
          customerName: 'Robert Johnson',
          membershipId: 'M10045',
          orderDate: 'May 08, 2025, 04:18 PM',
          itemCount: 3,
          total: 47.97,
          status: 'Completed'
        },
        {
          orderNumber: '1090',
          customerName: 'Emma Wilson',
          membershipId: 'M10034',
          orderDate: 'May 08, 2025, 02:05 PM',
          itemCount: 1,
          total: 19.99,
          status: 'Completed'
        },
        {
          orderNumber: '1089',
          customerName: 'Michael Chen',
          membershipId: 'M10022',
          orderDate: 'May 08, 2025, 10:33 AM',
          itemCount: 4,
          total: 52.96,
          status: 'Cancelled'
        }
      ]
    };
  },
  methods: {
    fetchOrderDetails() {
      // Reset error message
      this.formError = '';

      // Validate input
      if (!this.claimCode.trim()) {
        this.formError = 'Please enter a claim code';
        return;
      }

      // For demo purposes, we'll use a hard-coded claim code
      if (this.claimCode === 'BOOK1234') {
        this.order = {
          orderNumber: '1094',
          orderDate: 'May 09, 2025, 10:15 AM',
          customerName: 'Sarah Miller',
          membershipId: 'M10082',
          status: 'Pending',
          discount: 5, // 5% discount
          items: [
            {
              title: 'The Great Gatsby',
              author: 'F. Scott Fitzgerald',
              isbn: '9780743273565',
              price: 14.99,
              format: 'Paperback'
            },
            {
              title: 'To Kill a Mockingbird',
              author: 'Harper Lee',
              isbn: '9780061120084',
              price: 12.99,
              format: 'Paperback'
            },
            {
              title: '1984',
              author: 'George Orwell',
              isbn: '9780451524935',
              price: 9.99,
              format: 'Mass Market Paperback'
            },
            {
              title: 'Pride and Prejudice',
              author: 'Jane Austen',
              isbn: '9780141439518',
              price: 7.99,
              format: 'Paperback'
            },
            {
              title: 'The Hobbit',
              author: 'J.R.R. Tolkien',
              isbn: '9780547928227',
              price: 16.99,
              format: 'Hardcover'
            }
          ]
        };
      } else {
        this.formError = 'Invalid claim code. Please try again.';
        this.order = null;
      }
    },
    calculateSubtotal() {
      if (!this.order) return 0;
      return this.order.items.reduce((acc, item) => acc + item.price, 0);
    },
    calculateDiscount() {
      if (!this.order || !this.order.discount) return 0;
      return this.calculateSubtotal() * (this.order.discount / 100);
    },
    calculateTotal() {
      return this.calculateSubtotal() - this.calculateDiscount();
    },
    fulfillOrder() {
      // In a real app, this would call an API
      // For demo purposes, we'll just update the local state
      this.order.status = 'Completed';

      // Show success message (in a real app, use a proper notification system)
      alert('Order #' + this.order.orderNumber + ' has been successfully fulfilled!');
    }
  }
};
</script>

<style scoped>
/* (your existing styles) */
</style>
